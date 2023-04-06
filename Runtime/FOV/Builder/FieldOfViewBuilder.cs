using Backstreets.Data;
using Backstreets.FOV.Geometry;
using Backstreets.FOV.Jobs;
using Backstreets.FOV.Jobs.SweepVisitors;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Backstreets.FOV
{
    public class FieldOfViewBuilder
    {
        private readonly IGeometrySource geometrySource;
        private FieldOfViewSpace space;
        private PocketID containingPocket;

        public FieldOfViewBuilder(IGeometrySource geometrySource)
        {
            this.geometrySource = geometrySource;
        }

        public void SetOrigin(float2 origin, PocketID pocket)
        {
            space = new FieldOfViewSpace(origin);
            containingPocket = pocket;
        }

        public JobPromise<FieldOfView> Build(Allocator outputAllocator)
        {
            JobPromise<BlockingGeometry> geometryPromise = FetchGeometryData(containingPocket);
            JobPromise<FieldOfView> fieldOfViewPromise = ScheduleFieldOfViewCalculation(in geometryPromise, outputAllocator);

            geometryPromise.Dispose(fieldOfViewPromise.Handle);

            return fieldOfViewPromise;
        }

        private JobPromise<BlockingGeometry> FetchGeometryData(PocketID pocket)
        {
            JobPromise<PocketGeometry> geometryPromise = geometrySource.GetGeometry(pocket);
            NativeArray<Line> edges = geometryPromise.Result.Lines;
            BlockingGeometry result = new(edges.Length * 2, Allocator.TempJob);

            JobHandle assemble = new BuildCornersJob(space, edges, result.Corners)
                .Schedule(arrayLength: edges.Length, innerloopBatchCount: 64, geometryPromise);

            { // Cleanup
                geometryPromise.Dispose(assemble);
            }

            return new JobPromise<BlockingGeometry>(assemble, result);
        }

        private JobPromise<FieldOfView> ScheduleFieldOfViewCalculation(
            in JobPromise<BlockingGeometry> geometry,
            Allocator outputAllocator)
        {
            int estimatedBoundsCount = geometry.Result.Corners.Length / 2 + 1;
            FieldOfView result = new(space, estimatedBoundsCount, outputAllocator);
            FieldOfViewBuilderVisitor builder = new(result);

            JobHandle buildBounds = SweepLineOfSight<FieldOfViewBuilderVisitor>.Sweep(builder, in geometry);

            return new JobPromise<FieldOfView>(buildBounds, result);
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
