using Backstreets.FOV.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Backstreets.FOV.Jobs
{
    [BurstCompile]
    internal struct RaycastLinesJob : IJob
    {
        public RaycastLinesJob(NativeArray<Corner> corners, float2 ray, LineOfSight hits)
        {
            this.corners = corners;
            this.ray = ray;
            this.hits = hits;
        }

        [ReadOnly] private readonly NativeArray<Corner> corners; // array may be unordered, since this job tests every element.
        [ReadOnly] private readonly float2 ray;
        private LineOfSight hits;

        public void Execute()
        {
            foreach (Corner corner in corners)
            {
                // each line has two corners, therefore it occurs twice in the corners list.
                bool isLineStart = corner.End == Corner.Endpoint.Right;
                if (isLineStart && IsHit(corner.Line))
                {
                    hits.AddObstacle(corner.Line);
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
}