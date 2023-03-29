using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Backstreets.Viewport.Jobs
{
    [BurstCompile]
    internal struct RaycastLinesJob : IJob
    {
        public RaycastLinesJob(NativeArray<Corner> corners, Vector2 ray, LineOfSight hits)
        {
            this.corners = corners;
            this.ray = ray;
            this.hits = hits;
        }

        [ReadOnly] private readonly NativeArray<Corner> corners; // array may be unordered, since this job tests every element.
        [ReadOnly] private readonly Vector2 ray;
        private LineOfSight hits;

        public void Execute()
        {
            foreach (Corner corner in corners)
            {
                (Vector2 left, Vector2 right) = Align(corner.Position, corner.Right);
                if (IsHit(left, right))
                {
                    LineOfSight.Obstacle obstacle = new() { Left = left, Right = right };
                    hits.AddObstacle(obstacle);
                }
            }
        }

        private bool IsHit(Vector2 left, Vector2 right)
        {
            float leftAngle = Vector2.SignedAngle(ray, left);
            float rightAngle = Vector2.SignedAngle(ray, right);
            return Math.Sign(leftAngle) != Math.Sign(rightAngle)
                   && rightAngle != 0 // Line ends at ray
                   && Math.Abs(rightAngle - leftAngle) < 180;
        }

        private static (Vector2 left, Vector2 right) Align(Vector2 x, Vector2 y) =>
            Corner.GetRelativeDirection(x, y) switch
            {
                Corner.RelativeDirection.Straight => (x, y),
                Corner.RelativeDirection.Left => (y, x),
                Corner.RelativeDirection.Right => (x, y),
                _ => throw new ArgumentOutOfRangeException()
            };
    }
}