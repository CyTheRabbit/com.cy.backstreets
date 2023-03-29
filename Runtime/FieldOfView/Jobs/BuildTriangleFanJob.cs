using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Backstreets.FieldOfView.Jobs
{
    [BurstCompile]
    internal struct BuildTriangleFanJob : IJob
    {
        public BuildTriangleFanJob(
            NativeList<Line> segments,
            FieldOfViewSpace space,
            NativeList<Vector3> vertices,
            NativeList<int> indexes)
        {
            this.segments = segments;
            this.space = space;
            this.vertices = vertices;
            this.indexes = indexes;
        }

        [ReadOnly] private readonly NativeList<Line> segments;
        [ReadOnly] private readonly FieldOfViewSpace space;
        [WriteOnly] private NativeList<Vector3> vertices;
        [WriteOnly] private NativeList<int> indexes;

        public void Execute()
        {
            NativeArray<Vector3> triangleVertices = new(2, Allocator.Temp);
            NativeArray<int> triangleIndices = new(3, Allocator.Temp);
            vertices.Add(space.ViewportToWorld(Vector2.zero));
            triangleIndices[0] = 2;
            triangleIndices[1] = 1;
            triangleIndices[2] = 0;
            foreach (Line segment in segments)
            {
                triangleVertices[0] = space.ViewportToWorld(segment.Left);
                triangleVertices[1] = space.ViewportToWorld(segment.Right);
                vertices.AddRange(triangleVertices);
                indexes.AddRange(triangleIndices);
                triangleIndices[0] += 2;
                triangleIndices[1] += 2;
            }

            triangleVertices.Dispose();
            triangleIndices.Dispose();
        }
    }
}