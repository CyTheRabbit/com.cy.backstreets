using System;
using Backstreets.Data;
using Backstreets.FOV.Geometry;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Backstreets.FOV.Builder
{
    internal struct FOVMeshBuilder : IDisposable
    {
        private NativeArray<BoundSector> fovSectors;
        private readonly FieldOfViewSpace fovSpace;
        private Mesh.MeshDataArray meshDataArray;
        private Mesh.MeshData MeshData => meshDataArray[0];
        private readonly int vertexCount;
        private readonly int indexCount;

        public FOVMeshBuilder(FieldOfView fov)
        {
            fovSectors = fov.GetAllBoundSectors(Allocator.TempJob);
            fovSpace = fov.Space;
            vertexCount = fovSectors.Length * 4;
            indexCount = fovSectors.Length * 6;


            meshDataArray = Mesh.AllocateWritableMeshData(meshCount: 1);
            
            MeshData.SetIndexBufferParams(indexCount, IndexFormat.UInt32);
            MeshData.SetVertexBufferParams(vertexCount, 
                new VertexAttributeDescriptor(VertexAttribute.Position),
                new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1),
                new VertexAttributeDescriptor(VertexAttribute.Color, dimension: 4, stream: 2));
        }

        public void Build(Mesh mesh)
        {
            Mesh.MeshData data = MeshData;
            data.subMeshCount = 1;
            data.SetSubMesh(0, new SubMeshDescriptor(indexStart: 0, indexCount));

            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        }

        public void Dispose()
        {
            fovSectors.Dispose();
            // meshDataArray is already disposed in the build method
        }

        public JobHandle InitIndices() => 
            new InitIndicesJob
            {
                Output = MeshData.GetIndexData<QuadIndices>()
            }.Schedule(fovSectors.Length, 32);

        public JobHandle InitVertices()
        {
            int stream = MeshData.GetVertexAttributeStream(VertexAttribute.Position);
            return new InitVerticesJob
            {
                Sectors = fovSectors,
                Space = fovSpace,
                Output = MeshData.GetVertexData<QuadVertices>(stream)
            }.Schedule(fovSectors.Length, 32);
        }

        public JobHandle InitNormals()
        {
            int stream = MeshData.GetVertexAttributeStream(VertexAttribute.Normal);
            return new InitNormalsJob
            {
                Output = MeshData.GetVertexData<float3>(stream)
            }.Schedule(vertexCount, 64);
        }

        public JobHandle PaintPocketColors(NativeParallelHashMap<PocketID, Color> palette)
        {
            int stream = MeshData.GetVertexAttributeStream(VertexAttribute.Color);
            return new PocketColorsJob
            {
                Sectors = fovSectors,
                Palette = palette,
                Output = MeshData.GetVertexData<Color>(stream),
            }.Schedule(arrayLength: vertexCount, innerloopBatchCount: 32);
        }


        private struct InitIndicesJob : IJobParallelFor
        {
            [WriteOnly] public NativeArray<QuadIndices> Output;

            public void Execute(int index)
            {
                const int verticesPerSector = 4;
                Output[index] = new QuadIndices(first: index * verticesPerSector, 0, 1, 2, 3, 2, 1);
            }
        }

        private struct InitVerticesJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<BoundSector> Sectors;
            [WriteOnly] public NativeArray<QuadVertices> Output;
            public FieldOfViewSpace Space;

            public void Execute(int index)
            {
                BoundSector sector = Sectors[index];
                Output[index] = new QuadVertices(
                    Convert(sector.Near.Right),
                    Convert(sector.Near.Left),
                    Convert(sector.Far.Right),
                    Convert(sector.Far.Left));
            }

            private float3 Convert(float2 vector) => math.float3(Space.ViewportToWorld(vector), 0);
        }

        private struct PocketColorsJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<BoundSector> Sectors;
            [ReadOnly] public NativeParallelHashMap<PocketID, Color> Palette;
            [WriteOnly] public NativeArray<Color> Output;

            public void Execute(int vertexIndex)
            {
                const int verticesPerSector = 4;

                int sectorIndex = vertexIndex / verticesPerSector;
                BoundSector sector = Sectors[sectorIndex];
                Color color = Palette[sector.Pocket];
                Output[vertexIndex] = color;
            }
        }

        private struct InitNormalsJob : IJobParallelFor
        {
            [WriteOnly] public NativeArray<float3> Output;

            public void Execute(int index)
            {
                Output[index] = new float3(0, 0, -1);
            }
        }


        private struct QuadVertices
        {
            // ReSharper disable NotAccessedField.Local
            public float3 A, B, C, D;
            // ReSharper restore NotAccessedField.Local

            public QuadVertices(float3 a, float3 b, float3 c, float3 d)
            {
                A = a;
                B = b;
                C = c;
                D = d;
            }
        }

        private struct QuadIndices
        {
            // ReSharper disable NotAccessedField.Local
            public int A, B, C, D, E, F;
            // ReSharper restore NotAccessedField.Local

            public QuadIndices(int first, int a, int b, int c, int d, int e, int f)
            {
                A = first + a;
                B = first + b;
                C = first + c;
                D = first + d;
                E = first + e;
                F = first + f;
            }
        }
    }
}