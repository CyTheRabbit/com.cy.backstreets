using Backstreets.FOV.Geometry;
using Backstreets.FOV.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV.Jobs
{
    [BurstCompile]
    internal struct GetCornerRangeJob : IJob
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