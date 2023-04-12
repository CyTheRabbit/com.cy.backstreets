using System.Collections.Generic;
using System.Linq;
using Backstreets.Data;
using Backstreets.FOV.Geometry;
using Backstreets.FOV.Jobs;
using Backstreets.FOV.Utility;
using Backstreets.Pocket;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.SceneManagement;

namespace Backstreets.FOV.Sandbox
{
    internal class SceneGeometrySource : IGeometrySource
    {
        private Scene scene;

        public SceneGeometrySource(Scene scene)
        {
            this.scene = scene;
        }

        public JobPromise<PocketGeometry> GetGeometry(PocketID pocketID)
        {
            PocketPrefabDetails pocket = GetScenePockets().SingleOrDefault(p => p.PocketID == pocketID);
            if (pocket == null) return JobPromise<PocketGeometry>.Complete(new PocketGeometry(length: 0));

            ViewportObstacle[] obstacles = pocket.GetComponentsInChildren<ViewportObstacle>().ToArray();
            int shapeCount = obstacles.Length;
            int edgesCount = obstacles.Sum(obstacle => obstacle.Vertices.Length);

            NativeArray<float2> vertices = new(edgesCount, Allocator.TempJob);
            NativeArray<IndexRange> shapeRanges = new(shapeCount, Allocator.TempJob);
            int nextAvailableIndex = 0;
            for (int i = 0; i < obstacles.Length; i++)
            {
                ViewportObstacle obstacle = obstacles[i];

                IndexRange shapeRange = new(nextAvailableIndex, nextAvailableIndex += obstacle.Vertices.Length);
                shapeRanges[i] = shapeRange;

                NativeSlice<float2> shapeVertices = vertices.Slice(shapeRange.Start, shapeRange.Length);
                shapeVertices.CopyFrom(obstacle.Vertices);
            }

            PocketGeometry result = new(edgesCount);
            JobHandle makeEdges = new MakeEdgesJob(vertices, shapeRanges, result.Edges)
                .Schedule(arrayLength: shapeCount, innerloopBatchCount: 8);

            { // Cleanup
                vertices.Dispose(makeEdges);
                shapeRanges.Dispose(makeEdges);
            }

            return new JobPromise<PocketGeometry>(makeEdges, result);
        }

        private IEnumerable<PocketPrefabDetails> GetScenePockets() =>
            scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<PocketPrefabDetails>());


        [BurstCompile]
        private struct MakeEdgesJob : IJobParallelFor
        {
            [ReadOnly] private readonly NativeArray<float2> vertices;
            [ReadOnly] private readonly NativeArray<IndexRange> shapes;
            [WriteOnly] private NativeArray<Line> edges;

            public MakeEdgesJob(NativeArray<float2> vertices, NativeArray<IndexRange> shapes, NativeArray<Line> edges)
            {
                this.vertices = vertices;
                this.shapes = shapes;
                this.edges = edges;
            }

            public void Execute(int shapeIndex)
            {
                IndexRange shape = shapes[shapeIndex];
                NativeSlice<float2> shapeVertices = vertices.Slice(shape.Start, shape.Length);
                NativeArray<Line> shapeEdges = edges.GetSubArray(shape.Start, shape.Length);
                NativeArray<float2> edgeCorners = shapeEdges.Reinterpret<float2>(UnsafeUtility.SizeOf<Line>());
                for (int i = 0; i < shapeVertices.Length - 1; i++)
                {
                    float2 vertex = shapeVertices[i];
                    int endOfPreviousEdge = i * 2 + 1;
                    int startOfNextEdge = (i + 1) * 2;
                    edgeCorners[endOfPreviousEdge] = edgeCorners[startOfNextEdge] = vertex;
                }

                { // Loop around on itself
                    edgeCorners[^1] = edgeCorners[0] = shapeVertices[^1];
                }
            }
        }
    }
}