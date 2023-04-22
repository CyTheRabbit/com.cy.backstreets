using Backstreets.FOV.Geometry;
using Backstreets.FOV.Jobs.SweepRecorders;
using Backstreets.FOV.Jobs.SweepFilters;
using Backstreets.FOV.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Backstreets.FOV.Jobs
{
    internal static class SweepLineOfSight<TRecorder, TFilter>
        where TRecorder : struct, ISweepRecorder
        where TFilter : struct, ISweepFilter
    {
        internal static JobHandle Sweep(
            TRecorder recorder,
            TFilter filter,
            in JobPromise<BlockingGeometry> geometry)
        {
            NativeArray<Corner> corners = geometry.Result.Corners;
            NativeArray<Corner> orderedCorners = new(corners.Length, Allocator.TempJob);
            NativeReference<IndexRange> indexRange = new(Allocator.TempJob);
            LineOfSight lineOfSight = new(capacity: 16);

            JobHandle copyCorners = new CopyArrayJob<Corner>(corners, orderedCorners).Schedule(geometry);
            JobHandle orderCorners = orderedCorners.SortJob(new Corner.CompareByAngle()).Schedule(copyCorners);
            JobHandle narrowIndexRange =
                new GetIndexRangeJob(orderedCorners, indexRange, filter.RightLimit, filter.LeftLimit)
                    .Schedule(orderCorners);
            JobHandle prepareStartingLineOfSight =
                new RaycastEdgesJob(filter, corners, LineMath.Ray(filter.RightLimit), lineOfSight)
                    .Schedule(geometry);
            JobHandle preparationJobs = JobHandle.CombineDependencies(prepareStartingLineOfSight, narrowIndexRange);

            JobHandle sweep = new SweepJob(recorder, filter, lineOfSight, orderedCorners, indexRange)
                .Schedule(preparationJobs);

            {
                // Cleanup
                orderedCorners.Dispose(sweep);
                lineOfSight.Dispose(sweep);
                indexRange.Dispose(sweep);
            }

            return sweep;
        }


        [BurstCompile]
        private struct SweepJob : IJob
        {
            public SweepJob(
                TRecorder recorder,
                TFilter filter,
                LineOfSight lineOfSight,
                NativeArray<Corner> corners,
                NativeReference<IndexRange> indexRange)
            {
                this.recorder = recorder;
                this.filter = filter;
                this.lineOfSight = lineOfSight;
                this.corners = corners;
                this.indexRange = indexRange;
            }

            private TRecorder recorder;
            private TFilter filter;
            private LineOfSight lineOfSight;
            [ReadOnly] private readonly NativeReference<IndexRange> indexRange;
            [ReadOnly] private readonly NativeArray<Corner> corners;
    
            public void Execute()
            {
                lineOfSight.LookAt(LineMath.Ray(filter.RightLimit));
                recorder.Start(in lineOfSight);

                IndexRange range = indexRange.Value;
                if (range.Length >= 0)
                {
                    SweepRange(range);
                }
                else
                {
                    SweepRange(new IndexRange(range.Start, corners.Length));
                    SweepRange(new IndexRange(0, range.End));
                }
    
                lineOfSight.LookAt(LineMath.Ray(filter.LeftLimit));
                recorder.End(in lineOfSight);
            }
    
            private void SweepRange(IndexRange range)
            {
                foreach (Corner corner in corners.Slice(range.Start, range.Length))
                {
                    if (!filter.ShouldProcess(corner.Edge)) continue;
    
                    lineOfSight.LookAt(corner);
                    recorder.PreUpdate(in lineOfSight);
                    LineOfSight.UpdateReport update = lineOfSight.Update(corner);
                    recorder.Record(in lineOfSight, update, corner);
                }
            }
        }

        [BurstCompile]
        private struct RaycastEdgesJob : IJob
        {
            public RaycastEdgesJob(TFilter filter, NativeArray<Corner> corners, float2 ray, LineOfSight lineOfSight)
            {
                this.filter = filter;
                this.corners = corners;
                this.ray = ray;
                this.lineOfSight = lineOfSight;
            }

            private TFilter filter;
            [ReadOnly] private readonly NativeArray<Corner> corners; // array may be unordered, since this job tests every element.
            [ReadOnly] private readonly float2 ray;
            private LineOfSight lineOfSight;

            public void Execute()
            {
                foreach (Corner corner in corners)
                {
                    // each line has two corners, therefore it occurs twice in the corners list.
                    if (corner.End == Corner.Endpoint.Left) continue;
                    if (!filter.ShouldProcess(corner.Edge)) continue;

                    if (IsHit(corner.Edge))
                    {
                        lineOfSight.AddEdge(corner.Edge, corner.EdgeIndex);
                    }
                }
            }

            private bool IsHit(Line edge)
            {
                LineMath.RayDomain rightDomain = LineMath.GetDomain(ray, edge.Right);
                LineMath.RayDomain leftDomain = LineMath.GetDomain(ray, edge.Left);
                return rightDomain is LineMath.RayDomain.Right or LineMath.RayDomain.Straight
                       && leftDomain is LineMath.RayDomain.Left;
            }
        }

        [BurstCompile]
        private struct GetIndexRangeJob : IJob
        {
            [ReadOnly] private NativeArray<Corner> orderedCorners;
            [WriteOnly] private NativeReference<IndexRange> indexRange;
            private readonly float rightLimit;
            private readonly float leftLimit;

            public GetIndexRangeJob(
                NativeArray<Corner> orderedCorners,
                NativeReference<IndexRange> indexRange,
                float rightLimit,
                float leftLimit)
            {
                this.orderedCorners = orderedCorners;
                this.indexRange = indexRange;
                this.rightLimit = rightLimit;
                this.leftLimit = leftLimit;
            }

            public void Execute()
            {
                (int Index, float Angle) rightBest = (orderedCorners.Length, 180);
                (int Index, float Angle) leftBest = (0, -180);
                for (int i = 0; i < orderedCorners.Length; i++)
                {
                    float angle = orderedCorners[i].Angle;
                    if (angle > rightLimit && angle < rightBest.Angle) rightBest = (Index: i, Angle: angle);
                    if (angle <= leftLimit && angle >= leftBest.Angle) leftBest = (Index: i, Angle: angle);
                }

                indexRange.Value = new IndexRange
                {
                    Start = rightBest.Index,
                    End = leftBest.Index + 1,
                };
            }
        }
    }
}