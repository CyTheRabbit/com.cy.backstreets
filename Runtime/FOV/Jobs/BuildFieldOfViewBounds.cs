using Backstreets.FOV.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Backstreets.FOV.Jobs
{
    [BurstCompile]
    internal struct BuildFieldOfViewBounds : IJob
    {
        public BuildFieldOfViewBounds(LineOfSight lineOfSight, NativeArray<Corner> corners, NativeList<Line> bounds)
        {
            this.lineOfSight = lineOfSight;
            this.corners = corners;
            this.bounds = bounds;
        }

        private LineOfSight lineOfSight;
        [ReadOnly] private readonly NativeArray<Corner> corners;
        [WriteOnly] private readonly NativeList<Line> bounds;

        public void Execute()
        {
            SegmentBuilder builder = new(bounds);
            builder.StartSegment(lineOfSight.Raycast(Vector2.left));
            foreach (Corner corner in corners)
            {
                float2 ray =  math.normalize(corner.Position);
                float2 hit = lineOfSight.Raycast(ray);
                LineOfSight.UpdateReport report = lineOfSight.Update(corner);
                if (report.ClosestObstacleChanged)
                {
                    builder.EndSegment(hit);
                    builder.StartSegment(lineOfSight.Raycast(ray));
                }
            }

            builder.EndSegment(lineOfSight.Raycast(Vector2.left));
        }


        private ref struct SegmentBuilder
        {
            private NativeList<Line> output;
            private float2 right;

            public SegmentBuilder(NativeList<Line> output)
            {
                this.output = output;
                right = float2.zero;
            }

            internal void StartSegment(float2 start)
            {
                right = start;
            }

            internal void EndSegment(float2 end)
            {
                Line segment = new(right, end);
                output.Add(segment);
            }
        }
    }
}