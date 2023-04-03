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
            FieldOfView fieldOfView,
            FanMeshData meshData)
        {
            this.fieldOfView = fieldOfView;
            this.meshData = meshData;
        }

        [ReadOnly] private readonly FieldOfView fieldOfView;
        [WriteOnly] private FanMeshData meshData;
 
        public void Execute()
        {
            NativeArray<float3> triangleVertices = new(2, Allocator.Temp);
            NativeArray<int> triangleIndices = new(3, Allocator.Temp);
            meshData.Vertices.Add(ToWorld(float2.zero));
            triangleIndices[0] = 2;
            triangleIndices[1] = 1;
            triangleIndices[2] = 0;
            foreach (Line bound in fieldOfView.Bounds)
            {
                triangleVertices[0] = ToWorld(bound.Left);
                triangleVertices[1] = ToWorld(bound.Right);
                meshData.Vertices.AddRange(triangleVertices);
                meshData.Indices.AddRange(triangleIndices);
                triangleIndices[0] += 2;
                triangleIndices[1] += 2;
            }

            triangleVertices.Dispose();
            triangleIndices.Dispose();
        }

        private float3 ToWorld(float2 point)
        {
            float2 worldPoint = fieldOfView.Space.ViewportToWorld(point);
            return new float3(worldPoint.x, worldPoint.y, 0);
        }
    }
}