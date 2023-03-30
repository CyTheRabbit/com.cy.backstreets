using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Backstreets.FieldOfView.LineMath;

namespace Backstreets.FieldOfView.Jobs
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
                Line obstacle = Align(corner.Position, corner.Right);
                if (IsHit(obstacle))
                {
                    hits.AddObstacle(obstacle);
                }
            }
        }

        private bool IsHit(Line obstacle)
        {
            float leftAngle = Vector2.SignedAngle(ray, obstacle.Left);
            float rightAngle = Vector2.SignedAngle(ray, obstacle.Right);
            return Math.Sign(leftAngle) != Math.Sign(rightAngle)
                   && rightAngle != 0 // Line ends at ray
                   && Math.Abs(rightAngle - leftAngle) < 180;
        }

        private static Line Align(float2 x, float2 y) =>
            GetDomain(x, y) switch
            {
                RayDomain.Straight => new Line(x, y),
                RayDomain.Left => new Line(y, x),
                RayDomain.Right => new Line(x, y),
                _ => throw new ArgumentOutOfRangeException()
            };
    }
}