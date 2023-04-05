using Backstreets.FOV.Geometry;
using Backstreets.FOV.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV.Jobs
{
    [BurstCompile]
    internal struct SweepLineOfSightJob<TVisitor> : IJob
        where TVisitor : struct, ILineOfSightVisitor
    {
        public SweepLineOfSightJob(
            LineOfSight lineOfSight,
            NativeArray<Corner> corners,
            NativeReference<IndexRange> indexRange,
            TVisitor visitor)
        {
            this.lineOfSight = lineOfSight;
            this.corners = corners;
            this.visitor = visitor;
            this.indexRange = indexRange;
        }

        private LineOfSight lineOfSight;
        private TVisitor visitor;
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
}