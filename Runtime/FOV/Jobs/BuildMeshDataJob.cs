using Backstreets.FOV.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Backstreets.FOV.Jobs
{
    [BurstCompile]
    internal struct BuildMeshDataJob : IJob
    {
        public BuildMeshDataJob(
            NativeArray<BoundSector> sectors,
            FieldOfViewSpace space,
            FanMeshData meshData)
        {
            this.sectors = sectors;
            this.space = space;
            this.meshData = meshData;
        }

        private readonly FieldOfViewSpace space;
        [ReadOnly] private readonly NativeArray<BoundSector> sectors;
        [WriteOnly] private FanMeshData meshData;
 
        public void Execute()
        {
            {
                NativeArray<float3> quadVertices = new(length: VerticesPerQuad, Allocator.Temp);
                NativeArray<int> quadIndices = new(length: IndicesPerQuad, Allocator.Temp);

                InitQuadIndices(ref quadIndices);
                foreach (BoundSector sector in sectors)
                {
                    ReadQuadVertices(ref quadVertices, in sector, in space);
                    AdvanceQuadIndices(ref quadIndices);
                    RecordQuad(ref meshData, in quadVertices, in quadIndices);
                }

                quadVertices.Dispose();
                quadIndices.Dispose();
            }

            static void ReadQuadVertices(
                ref NativeArray<float3> vertices, in BoundSector sector, in FieldOfViewSpace space)
            {
                vertices[0] = ToWorld(sector.Near.Right, space);
                vertices[1] = ToWorld(sector.Near.Left, space);
                vertices[2] = ToWorld(sector.Far.Right, space);
                vertices[3] = ToWorld(sector.Far.Left, space);
            }

            static float3 ToWorld(float2 point, FieldOfViewSpace space)
            {
                float2 worldPoint = space.ViewportToWorld(point);
                return new float3(worldPoint.x, worldPoint.y, 0);
            }

            static void InitQuadIndices(ref NativeArray<int> indices)
            {
                indices[0] = 0 - VerticesPerQuad;
                indices[1] = 1 - VerticesPerQuad;
                indices[2] = 2 - VerticesPerQuad;
                indices[3] = 3 - VerticesPerQuad;
                indices[4] = 2 - VerticesPerQuad;
                indices[5] = 1 - VerticesPerQuad;
            }
            
            static void AdvanceQuadIndices(ref NativeArray<int> indices)
            {
                for (int i = 0; i < indices.Length; i++) indices[i] += VerticesPerQuad;
            }

            static void RecordQuad(ref FanMeshData mesh, in NativeArray<float3> vertices, in NativeArray<int> indices)
            {
                mesh.Vertices.AddRange(vertices);
                mesh.Indices.AddRange(indices);
            }
        }

        private const int VerticesPerQuad = 4;
        private const int IndicesPerQuad = 6;
    }
}