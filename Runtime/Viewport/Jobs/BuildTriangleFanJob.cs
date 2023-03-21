using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Backstreets.Viewport.Jobs
{
    [BurstCompile]
    internal struct BuildTriangleFanJob : IJob
    {
        public BuildTriangleFanJob(
            NativeList<ViewportLine> edges,
            ViewportSpace space,
            NativeList<Vector3> vertices,
            NativeList<int> indexes)
        {
            this.edges = edges;
            this.space = space;
            this.vertices = vertices;
            this.indexes = indexes;
        }

        [ReadOnly] private readonly NativeList<ViewportLine> edges;
        [ReadOnly] private readonly ViewportSpace space;
        [WriteOnly] private NativeList<Vector3> vertices;
        [WriteOnly] private NativeList<int> indexes;

        public void Execute()
        {
            NativeArray<Vector3> triangleVertices = new(2, Allocator.Temp);
            NativeArray<int> triangleIndices = new(3, Allocator.Temp);
            vertices.Add(space.Origin);
            triangleIndices[0] = 0;
            triangleIndices[1] = 2;
            triangleIndices[2] = 1;
            foreach (ViewportLine edge in edges)
            {
                triangleVertices[0] = space.Convert(edge.Left);
                triangleVertices[1] = space.Convert(edge.Right);
                vertices.AddRange(triangleVertices);
                indexes.AddRange(triangleIndices);
                triangleIndices[1] += 2;
                triangleIndices[2] += 2;
            }

            triangleVertices.Dispose();
            triangleIndices.Dispose();
        }
    }
}