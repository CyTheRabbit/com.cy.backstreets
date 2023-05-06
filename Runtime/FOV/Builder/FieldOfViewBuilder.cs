using System.Collections.Generic;
using Backstreets.Data;
using Backstreets.FOV.Builder.SweepFilters;
using Backstreets.FOV.Builder.SweepRecorders;
using Backstreets.FOV.Geometry;
using Backstreets.FOV.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Backstreets.FOV.Builder
{
    public class FieldOfViewBuilder
    {
        private readonly IGeometrySource geometrySource;
        private readonly PotentiallyVisibleSetBuilder potentiallyVisibleSetBuilder;
        private FieldOfViewSpace space;
        private PocketID containingPocket;
        private readonly Dictionary<PocketID, JobPromise<BlockingGeometry>> sweepGeometryCache;
        private readonly List<JobPromise<FarBounds>> farBoundsCache;
        private JobPromise<FarBounds> originBoundsPromise;

        public FieldOfViewBuilder(IGeometrySource geometrySource)
        {
            this.geometrySource = geometrySource;
            potentiallyVisibleSetBuilder = new PotentiallyVisibleSetBuilder(geometrySource);
            sweepGeometryCache = new Dictionary<PocketID, JobPromise<BlockingGeometry>>(capacity: 16);
            farBoundsCache = new List<JobPromise<FarBounds>>(capacity: 64);
        }

        public void SetOrigin(float2 origin, PocketID pocket)
        {
            space = new FieldOfViewSpace(origin);
            containingPocket = pocket;
        }

        public JobPromise<FieldOfView> Build(Allocator outputAllocator)
        {
            FieldOfView fov = new(space, outputAllocator);
            JobPromise<FieldOfView> fovPromise = JobPromise<FieldOfView>.Complete(fov);

            PotentiallyVisibleSet potentiallyVisibleSet = potentiallyVisibleSetBuilder.Build(Allocator.TempJob, containingPocket, space);
            
            foreach (PocketID pocket in potentiallyVisibleSet.VisiblePockets)
            {
                sweepGeometryCache[pocket] = FetchGeometryData(pocket);
            }


            originBoundsPromise = SweepOrigin(sweepGeometryCache[containingPocket]);
            {
                using JobPromise<WindowBounds> originSectorsPromise = BuildOriginSectors(in originBoundsPromise);
                fovPromise = AppendWindowSectors(fovPromise.Reuse(), in originSectorsPromise);
                originSectorsPromise.Dispose(fovPromise);
            }

            foreach (PotentiallyVisibleSet.Window window in potentiallyVisibleSet.VisibleWindows)
            {
                JobPromise<FarBounds> farBoundsPromise = SweepWindow(window, sweepGeometryCache[window.Exit]);
                farBoundsCache.Add(farBoundsPromise);

                using JobPromise<WindowBounds> sectorsPromise = BuildWindowSectors(window);
                fovPromise = AppendWindowSectors(fovPromise.Reuse(), in sectorsPromise);
                sectorsPromise.Dispose(fovPromise);
            }

            { // Cleanup
                potentiallyVisibleSet.Dispose(fovPromise);
                ClearGeometryPromises(fovPromise);
                originBoundsPromise.Dispose(fovPromise);

                foreach (JobPromise<FarBounds> promise in farBoundsCache)
                {
                    promise.Dispose(fovPromise);
                }

                farBoundsCache.Clear();
            }

            return fovPromise;
        }

        private JobPromise<WindowBounds> BuildOriginSectors(in JobPromise<FarBounds> boundsPromise)
        {
            WindowBounds result = new(Allocator.TempJob);
            JobHandle handle = BoundSectorsBuilder<QueueSectorRecorder>.SweepFromOrigin(
                new QueueSectorRecorder(result.Sectors),
                containingPocket,
                in boundsPromise);
            return new JobPromise<WindowBounds>(handle, result);
        }

        private JobPromise<FieldOfView> AppendWindowSectors(
            ReusePromise<FieldOfView> reuseFOV,
            in JobPromise<WindowBounds> sectorsPromise)
        {
            FieldOfView fov = reuseFOV.Value;
            WindowBounds window = sectorsPromise.Result;

            JobHandle inputDeps = JobHandle.CombineDependencies(reuseFOV, sectorsPromise);
            JobHandle handle = new CopyQueueJob<BoundSector>(window.Sectors, fov.Sectors).Schedule(inputDeps);
            return new JobPromise<FieldOfView>(handle, fov);
        }

        private JobPromise<WindowBounds> BuildWindowSectors(PotentiallyVisibleSet.Window window)
        {
            JobPromise<FarBounds> childPromise = farBoundsCache[window.Index];
            JobPromise<FarBounds> parentPromise =
                window.HasParent ? farBoundsCache[window.ParentIndex] : originBoundsPromise;
            WindowBounds result = new(Allocator.TempJob);
            JobHandle handle = BoundSectorsBuilder<QueuePortalSectorRecorder>.SweepSectors(
                new QueuePortalSectorRecorder(result.Sectors, window.Portal.EdgeIndex),
                window.Exit,
                in parentPromise,
                in childPromise);

            JobPromise<WindowBounds> windowPromise = new(handle, result);
            return windowPromise;
        }

        private void ClearGeometryPromises(JobPromise<FieldOfView> fovPromise)
        {
            // Assuming each geometry promise gets used during FOV construction
            foreach (JobPromise<BlockingGeometry> geometryPromise in sweepGeometryCache.Values)
            {
                geometryPromise.Dispose(fovPromise);
            }

            sweepGeometryCache.Clear();
        }

        private JobPromise<BlockingGeometry> FetchGeometryData(PocketID pocket)
        {
            PocketGeometry geometry = geometrySource.GetGeometry(pocket);
            if (!geometry.IsValid) return new JobPromise<BlockingGeometry>();
            NativeArray<Line> edges = geometry.Edges;
            BlockingGeometry result = new(edges.Length * 2, Allocator.TempJob);

            JobHandle assemble = new BuildCornersJob(space, edges, result.Corners)
                .Schedule(arrayLength: edges.Length, innerloopBatchCount: 64);
            JobHandle sort = result.Corners.SortJob(new Corner.CompareByAngle())
                .Schedule(assemble);

            return new JobPromise<BlockingGeometry>(sort, result);
        }

        private JobPromise<FarBounds> SweepOrigin(in JobPromise<BlockingGeometry> geometry)
        {
            FarBounds result = new(Allocator.TempJob);
            JobHandle sweepHandle = SweepLineOfSight<QueueRecorder, FullTurn>.Sweep(
                recorder: new QueueRecorder(result.Bounds),
                filter: new FullTurn(),
                geometry);
            return new JobPromise<FarBounds>(sweepHandle, result);
        }

        private JobPromise<FarBounds> SweepWindow(
            PotentiallyVisibleSet.Window window,
            in JobPromise<BlockingGeometry> geometry)
        {
            FarBounds result = new(Allocator.TempJob);
            JobHandle sweepHandle = SweepLineOfSight<QueueRecorder, PortalFilter>.Sweep(
                recorder: new QueueRecorder(result.Bounds),
                filter: new PortalFilter(window.ExitBound),
                geometry);
            return new JobPromise<FarBounds>(sweepHandle, result);
        }
    }


    /// <remarks>
    /// Wrapper for <see cref="NativeArray{T}"/>, as it doesn't implement <see cref="INativeDisposable"/> for some
    /// reason.
    /// </remarks>
    internal struct BlockingGeometry : INativeDisposable
    {
        public NativeArray<Corner> Corners;

        public BlockingGeometry(int length, Allocator allocator) =>
            Corners = new NativeArray<Corner>(length, allocator);

        public void Dispose() => Corners.Dispose();

        public JobHandle Dispose(JobHandle inputDeps) => Corners.Dispose(inputDeps);
    }
}
