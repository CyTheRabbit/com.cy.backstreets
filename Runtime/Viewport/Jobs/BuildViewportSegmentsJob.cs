using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Backstreets.Viewport.Jobs
{
    [BurstCompile]
    internal struct BuildViewportSegmentsJob : IJob
    {
        public BuildViewportSegmentsJob(LineOfSight lineOfSight, NativeArray<Corner> corners, NativeList<ViewportSegment> segments)
        {
            this.lineOfSight = lineOfSight;
            this.corners = corners;
            this.segments = segments;
        }

        private LineOfSight lineOfSight;
        [ReadOnly] private readonly NativeArray<Corner> corners;
        [WriteOnly] private readonly NativeList<ViewportSegment> segments;

        public void Execute()
        {
            SegmentBuilder builder = new(segments);
            builder.StartSegment(lineOfSight.Raycast(Vector2.left));
            foreach (Corner corner in corners)
            {
                Vector2 ray = corner.Position.normalized;
                Vector2 hit = lineOfSight.Raycast(ray);
                LineOfSight.UpdateReport report = UpdateLineOfSight(corner);
                if (report.ClosestObstacleChanged)
                {
                    builder.EndSegment(hit);
                    builder.StartSegment(lineOfSight.Raycast(ray));
                }
            }

            builder.EndSegment(lineOfSight.Raycast(Vector2.right));
        }

        private LineOfSight.UpdateReport UpdateLineOfSight(Corner corner)
        {
            LineOfSight.UpdateReport leftReport = UpdateLineOfSight(corner.Position, corner.Left);
            LineOfSight.UpdateReport rightReport = UpdateLineOfSight(corner.Position, corner.Right);
            return leftReport + rightReport;
        }

        private LineOfSight.UpdateReport UpdateLineOfSight(Vector2 vertex, Vector2 companion)
        {
            Corner.RelativeDirection lineDirection = Corner.GetRelativeDirection(vertex, companion);
            bool lineInvisible = lineDirection is Corner.RelativeDirection.Straight;
            if (lineInvisible) return default;

            bool lineStarts = lineDirection is Corner.RelativeDirection.Right;
            if (lineStarts)
            {
                LineOfSight.Obstacle obstacle = new() { Left = vertex, Right = companion };
                return lineOfSight.AddObstacle(obstacle);
            }
            else
            {
                LineOfSight.Obstacle obstacle = new() { Left = companion, Right = vertex };
                return lineOfSight.RemoveObstacle(obstacle);
            }
        }


        private ref struct SegmentBuilder
        {
            private NativeList<ViewportSegment> output;
            private Vector2 left;

            public SegmentBuilder(NativeList<ViewportSegment> output)
            {
                this.output = output;
                left = Vector2.zero;
            }

            internal void StartSegment(Vector2 start)
            {
                left = start;
            }

            internal void EndSegment(Vector2 end)
            {
                ViewportSegment segment = new() { Left = left, Right = end };
                output.Add(segment);
            }
        }
    }
}