using Backstreets.FOV.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Backstreets.FOV.Jobs
{
    [BurstCompile]
    internal struct BuildTriangleFanJob : IJob
    {
        public BuildTriangleFanJob(
            NativeList<Line> segments,
            FieldOfViewSpace space,
            NativeList<float3> vertices,
            NativeList<int> indexes)
        {
            this.segments = segments;
            this.space = space;
            this.vertices = vertices;
            this.indexes = indexes;
        }

        [ReadOnly] private readonly NativeList<Line> segments;
        [ReadOnly] private readonly FieldOfViewSpace space;
        [WriteOnly] private NativeList<float3> vertices;
        [WriteOnly] private NativeList<int> indexes;

        public void Execute()
        {
            NativeArray<float3> triangleVertices = new(2, Allocator.Temp);
            NativeArray<int> triangleIndices = new(3, Allocator.Temp);
            vertices.Add(ToWorld(float2.zero));
            triangleIndices[0] = 2;
            triangleIndices[1] = 1;
            triangleIndices[2] = 0;
            foreach (Line segment in segments)
            {
                triangleVertices[0] = ToWorld(segment.Left);
                triangleVertices[1] = ToWorld(segment.Right);
                vertices.AddRange(triangleVertices);
                indexes.AddRange(triangleIndices);
                triangleIndices[0] += 2;
                triangleIndices[1] += 2;
            }

            triangleVertices.Dispose();
            triangleIndices.Dispose();
        }

        private float3 ToWorld(float2 point)
        {
            float2 worldPoint = space.ViewportToWorld(point);
            return new float3(worldPoint.x, worldPoint.y, 0);
        }
    }
}