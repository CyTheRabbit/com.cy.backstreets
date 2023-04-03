using Backstreets.FOV.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Backstreets.FOV.Jobs
{
    [BurstCompile]
    internal struct SweepLineOfSightJob<TVisitor> : IJob
        where TVisitor : struct, ILineOfSightVisitor
    {
        public SweepLineOfSightJob(
            LineOfSight lineOfSight,
            NativeArray<Corner> corners,
            TVisitor visitor)
        {
            this.lineOfSight = lineOfSight;
            this.corners = corners;
            this.visitor = visitor;
        }

        private LineOfSight lineOfSight;
        private TVisitor visitor;
        [ReadOnly] private readonly NativeArray<Corner> corners;

        public void Execute()
        {
            lineOfSight.LookAt(Left);
            visitor.Start(in lineOfSight);
            foreach (Corner corner in corners)
            {
                if (!visitor.ShouldProcess(corner.Line)) continue;

                lineOfSight.LookAt(corner);
                visitor.PreUpdate(in lineOfSight);
                LineOfSight.UpdateReport update = lineOfSight.Update(corner);
                visitor.Update(in lineOfSight, update);
            }

            lineOfSight.LookAt(Left);
            visitor.End(in lineOfSight);
        }


        private static float2 Left => new(-1, 0);
    }
}