using Backstreets.FOV.Geometry;
using Backstreets.FOV.Jobs.SweepRecorders;
using Backstreets.FOV.Jobs.SweepFilters;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

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
            return new SweepJob(recorder, filter, corners).Schedule(geometry);
        }


        [BurstCompile]
        private struct SweepJob : IJob
        {
            public SweepJob(
                TRecorder recorder,
                TFilter filter,
                NativeArray<Corner> corners)
            {
                this.recorder = recorder;
                this.filter = filter;
                this.corners = corners;
            }

            private TRecorder recorder;
            private TFilter filter;
            [ReadOnly] private readonly NativeArray<Corner> corners;
    
            public void Execute()
            {
                LineOfSight lineOfSight = new(capacity: 16, allocator: Allocator.Temp);

                SweepDontRecord(ref lineOfSight);
                Sweep(ref lineOfSight);

                lineOfSight.Dispose();
            }

            private void SweepDontRecord(ref LineOfSight lineOfSight)
            {
                foreach (Corner corner in corners)
                {
                    if (!filter.ShouldProcess(corner.Edge)) continue;

                    lineOfSight.LookAt(corner);
                    lineOfSight.Update(corner);
                }
            }

            private void Sweep(ref LineOfSight lineOfSight)
            {
                lineOfSight.LookAt(LineMath.Left);
                recorder.Start(in lineOfSight);

                foreach (Corner corner in corners)
                {
                    if (!filter.ShouldProcess(corner.Edge)) continue;
    
                    lineOfSight.LookAt(corner);
                    recorder.PreUpdate(in lineOfSight);
                    LineOfSight.UpdateReport update = lineOfSight.Update(corner);
                    recorder.Record(in lineOfSight, update, corner);
                }

                lineOfSight.LookAt(LineMath.Left);
                recorder.End(in lineOfSight);
            }
        }
    }
}