using System.Collections.Generic;
using System.Linq;
using Backstreets.FOV.Geometry;
using Backstreets.FOV.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Backstreets.FOV
{
    public static class FieldOfViewBuilder
    {
        public static JobPromise<FieldOfView> Build(
            float2 origin, 
            IReadOnlyList<float2[]> shapes)
        {
            FieldOfViewSpace space = new(origin);

            JobPromise<BlockingGeometry> geometryPromise = ScheduleDataAssembly(shapes, space);
            JobPromise<FieldOfView> fieldOfViewPromise = ScheduleFieldOfViewCalculation(in geometryPromise, space);

            geometryPromise.Dispose(fieldOfViewPromise.Handle);

            return fieldOfViewPromise;
        }

        private static JobPromise<BlockingGeometry> ScheduleDataAssembly(
            IReadOnlyList<float2[]> shapes,
            FieldOfViewSpace space)
        {
            int shapesCount = shapes.Count;
            int totalVertexCount = shapes.Sum(shape => shape.Length);
            BlockingGeometry result = new(totalVertexCount, Allocator.TempJob);
            NativeArray<float2> source = new(totalVertexCount, Allocator.TempJob);
            NativeArray<int2> spans = new(shapesCount, Allocator.TempJob);

            int nextUnusedOutput = 0;
            for (int i = 0; i < shapesCount; i++)
            {
                float2[] obstacleVertices = shapes[i];
                int2 span = new(nextUnusedOutput, obstacleVertices.Length);
                source.Slice(span.x, span.y).CopyFrom(obstacleVertices);

                spans[i] = span;
                nextUnusedOutput += span.y;
            }

            JobHandle assemble = new BuildCornersJob(space, source, result.Corners, spans)
                .Schedule(arrayLength: spans.Length, innerloopBatchCount: 4);

            { // Cleanup
                source.Dispose(assemble);
                spans.Dispose(assemble);
            }

            return new JobPromise<BlockingGeometry>(assemble, result);
        }

        private static JobPromise<FieldOfView> ScheduleFieldOfViewCalculation(
            in JobPromise<BlockingGeometry> geometry,
            FieldOfViewSpace space)
        {
            NativeArray<Corner> corners = geometry.Result.Corners;
            NativeArray<Corner> orderedCorners = new(corners.Length, Allocator.TempJob);
            LineOfSight lineOfSight = new(capacity: 16);

            FieldOfView result = new(space, corners.Length + 1, Allocator.TempJob);

            JobHandle copyCorners = new CopyArrayJob<Corner>(corners, orderedCorners).Schedule(geometry);
            JobHandle orderCorners = orderedCorners.SortJob(new Corner.CompareByAngle()).Schedule(copyCorners);
            JobHandle raycastStartingLineOfSight =
                new RaycastLinesJob(corners, Vector2.left, lineOfSight).Schedule(geometry);
            JobHandle preparationJobs = JobHandle.CombineDependencies(raycastStartingLineOfSight, orderCorners);

            JobHandle buildBounds = new BuildFieldOfViewBounds(lineOfSight, orderedCorners, result.Bounds)
                .Schedule(preparationJobs);

            { // Cleanup
                orderedCorners.Dispose(buildBounds);
                lineOfSight.Dispose(buildBounds);
            }

            return new JobPromise<FieldOfView>(buildBounds, result);
        }

        /// <remarks>
        /// Wrapper for <see cref="NativeArray{T}"/>, as it doesn't implement <see cref="INativeDisposable"/> for some
        /// reason.
        /// </remarks>
        private struct BlockingGeometry : INativeDisposable
        {
            public NativeArray<Corner> Corners;

            public BlockingGeometry(int length, Allocator allocator)
            {
                Corners = new NativeArray<Corner>(length, allocator);
            }

            public void Dispose() => Corners.Dispose();

            public JobHandle Dispose(JobHandle inputDeps) => Corners.Dispose(inputDeps);
        }
    }
}
