using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Backstreets.FieldOfView.LineMath;

namespace Backstreets.FieldOfView.Jobs
{
    [BurstCompile]
    internal struct BuildViewportSegmentsJob : IJob
    {
        public BuildViewportSegmentsJob(LineOfSight lineOfSight, NativeArray<Corner> corners, NativeList<Line> segments)
        {
            this.lineOfSight = lineOfSight;
            this.corners = corners;
            this.segments = segments;
        }

        private LineOfSight lineOfSight;
        [ReadOnly] private readonly NativeArray<Corner> corners;
        [WriteOnly] private readonly NativeList<Line> segments;

        public void Execute()
        {
            SegmentBuilder builder = new(segments);
            builder.StartSegment(lineOfSight.Raycast(Vector2.left));
            foreach (Corner corner in corners)
            {
                float2 ray =  math.normalize(corner.Position);
                float2 hit = lineOfSight.Raycast(ray);
                LineOfSight.UpdateReport report = UpdateLineOfSight(corner);
                if (report.ClosestObstacleChanged)
                {
                    builder.EndSegment(hit);
                    builder.StartSegment(lineOfSight.Raycast(ray));
                }
            }

            builder.EndSegment(lineOfSight.Raycast(Vector2.left));
        }

        private LineOfSight.UpdateReport UpdateLineOfSight(Corner corner)
        {
            LineOfSight.UpdateReport leftReport = UpdateLineOfSight(corner.Position, corner.Left);
            LineOfSight.UpdateReport rightReport = UpdateLineOfSight(corner.Position, corner.Right);
            return leftReport + rightReport;
        }

        private LineOfSight.UpdateReport UpdateLineOfSight(float2 vertex, float2 companion)
        {
            RayDomain lineDirection = GetDomain(vertex, companion);
            bool lineInvisible = lineDirection is RayDomain.Straight;
            if (lineInvisible) return default;

            bool lineStarts = lineDirection is RayDomain.Right;
            if (lineStarts)
            {
                Line obstacle = new() { Left = vertex, Right = companion };
                return lineOfSight.AddObstacle(obstacle);
            }
            else
            {
                Line obstacle = new() { Left = companion, Right = vertex };
                return lineOfSight.RemoveObstacle(obstacle);
            }
        }


        private ref struct SegmentBuilder
        {
            private NativeList<Line> output;
            private float2 left;

            public SegmentBuilder(NativeList<Line> output)
            {
                this.output = output;
                left = float2.zero;
            }

            internal void StartSegment(float2 start)
            {
                left = start;
            }

            internal void EndSegment(float2 end)
            {
                Line segment = new(left, end);
                output.Add(segment);
            }
        }
    }
}