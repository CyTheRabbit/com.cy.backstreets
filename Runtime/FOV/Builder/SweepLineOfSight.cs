using Backstreets.FOV.Geometry;
using Backstreets.FOV.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Backstreets.FOV.Jobs
{
    internal static class SweepLineOfSight<TVisitor> where TVisitor : struct, ILineOfSightVisitor
    {
        internal static JobHandle Sweep(TVisitor visitor, in JobPromise<BlockingGeometry> geometry)
        {
            NativeArray<Corner> corners = geometry.Result.Corners;
            NativeArray<Corner> orderedCorners = new(corners.Length, Allocator.TempJob);
            NativeReference<IndexRange> indexRange = new(Allocator.TempJob);
            LineOfSight lineOfSight = new(capacity: 16);

            JobHandle copyCorners = new CopyArrayJob<Corner>(corners, orderedCorners).Schedule(geometry);
            JobHandle orderCorners = orderedCorners.SortJob(new Corner.CompareByAngle()).Schedule(copyCorners);
            JobHandle narrowIndexRange =
                new GetCornerRangeJob(orderedCorners, indexRange, visitor.RightLimit, visitor.LeftLimit)
                    .Schedule(orderCorners);
            JobHandle prepareStartingLineOfSight =
                new RaycastLinesJob(visitor, corners, LineMath.Ray(visitor.RightLimit), lineOfSight)
                    .Schedule(geometry);
            JobHandle preparationJobs = JobHandle.CombineDependencies(prepareStartingLineOfSight, narrowIndexRange);

            JobHandle sweep = new SweepJob(visitor, lineOfSight, orderedCorners, indexRange)
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
                TVisitor visitor,
                LineOfSight lineOfSight,
                NativeArray<Corner> corners,
                NativeReference<IndexRange> indexRange)
            {
                this.visitor = visitor;
                this.lineOfSight = lineOfSight;
                this.corners = corners;
                this.indexRange = indexRange;
            }

            private TVisitor visitor;
            private LineOfSight lineOfSight;
            [ReadOnly] private readonly NativeReference<IndexRange> indexRange;
            [ReadOnly] private readonly NativeArray<Corner> corners;
    
            public void Execute()
            {
                lineOfSight.LookAt(LineMath.Ray(visitor.RightLimit));
                visitor.Start(in lineOfSight);

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
    
                lineOfSight.LookAt(LineMath.Ray(visitor.LeftLimit));
                visitor.End(in lineOfSight);
            }
    
            private void SweepRange(IndexRange range)
            {
                foreach (Corner corner in corners.Slice(range.Start, range.Length))
                {
                    if (!visitor.ShouldProcess(corner.Line)) continue;
    
                    lineOfSight.LookAt(corner);
                    visitor.PreUpdate(in lineOfSight);
                    LineOfSight.UpdateReport update = lineOfSight.Update(corner);
                    visitor.Update(in lineOfSight, update, corner);
                }
            }
        }

        [BurstCompile]
        private struct RaycastLinesJob : IJob
        {
            public RaycastLinesJob(TVisitor visitor, NativeArray<Corner> corners, float2 ray, LineOfSight lineOfSight)
            {
                this.visitor = visitor;
                this.corners = corners;
                this.ray = ray;
                this.lineOfSight = lineOfSight;
            }

            private TVisitor visitor;
            [ReadOnly] private readonly NativeArray<Corner> corners; // array may be unordered, since this job tests every element.
            [ReadOnly] private readonly float2 ray;
            private LineOfSight lineOfSight;

            public void Execute()
            {
                foreach (Corner corner in corners)
                {
                    // each line has two corners, therefore it occurs twice in the corners list.
                    if (corner.End == Corner.Endpoint.Left) continue;
                    if (!visitor.ShouldProcess(corner.Line)) continue;

                    if (IsHit(corner.Line))
                    {
                        lineOfSight.AddObstacle(corner.Line);
                    }
                }
            }

            private bool IsHit(Line obstacle)
            {
                LineMath.RayDomain rightDomain = LineMath.GetDomain(ray, obstacle.Right);
                LineMath.RayDomain leftDomain = LineMath.GetDomain(ray, obstacle.Left);
                return rightDomain is LineMath.RayDomain.Right or LineMath.RayDomain.Straight
                       && leftDomain is LineMath.RayDomain.Left;
            }
        }

        [BurstCompile]
        private struct GetCornerRangeJob : IJob
        {
            [ReadOnly] private NativeArray<Corner> orderedCorners;
            [WriteOnly] private NativeReference<IndexRange> indexRange;
            private readonly float rightLimit;
            private readonly float leftLimit;

            public GetCornerRangeJob(
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