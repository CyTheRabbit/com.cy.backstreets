using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Backstreets.Viewport.Jobs
{
    [BurstCompile]
    internal struct BuildEdgesJob : IJob
    {
        public BuildEdgesJob(NativeArray<Vector2> vertices, ViewportSpace space, NativeArray<ViewportLine> edges)
        {
            this.vertices = vertices;
            this.space = space;
            this.edges = edges;
        }

        [ReadOnly] private readonly NativeArray<Vector2> vertices;
        [ReadOnly] private readonly ViewportSpace space;
        [WriteOnly] private NativeArray<ViewportLine> edges;

        public void Execute()
        {
            edges[0] = new ViewportLine
            {
                Left = space.Convert(vertices[^1]),
                Right = space.Convert(vertices[0])
            };
            for (int i = 1; i < vertices.Length; i++)
            {
                edges[i] = new ViewportLine
                {
                    Left = space.Convert(vertices[i - 1]),
                    Right = space.Convert(vertices[i])
                };
            }
        }
    }
}