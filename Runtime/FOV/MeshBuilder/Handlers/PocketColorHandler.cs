using System;
using Backstreets.Data;
using Backstreets.FOV.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Backstreets.FOV.MeshBuilder.Handlers
{
    internal class PocketColorHandler : IAttributeHandler
    {
        public VertexAttributeDescriptor MakeDescriptor(
            BuildRequest.AttributeType attribute,
            VertexAttribute output,
            int index) =>
            new(output, dimension: 4, stream: index);

        public void PopulateVertices(
            ref MeshBuildingContext context,
            ref Mesh.MeshData meshData,
            BuildRequest.AttributeType attribute,
            VertexAttribute output)
        {
            int stream = meshData.GetVertexAttributeStream(output);
            NativeParallelHashMap<PocketID, Color> palette = context.Request.DebugPalette ?? ThrowNullPalette();

            PocketColorsJob job = new()
            {
                Sectors = context.Sectors,
                Palette = palette,
                Output = meshData.GetVertexData<Color>(stream),
            };
            job.Run(context.VertexCount);
        }


        private static NativeParallelHashMap<PocketID, Color> ThrowNullPalette() => throw new ArgumentNullException(
            paramName: nameof(BuildRequest.DebugPalette), 
            message: "Debug palette is necessary to produce pocket colors attribute");


        [BurstCompile]
        private struct PocketColorsJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<BoundSector> Sectors;
            [ReadOnly] public NativeParallelHashMap<PocketID, Color> Palette;
            [WriteOnly] public NativeArray<Color> Output;

            public void Execute(int vertexIndex)
            {
                const int verticesPerSector = 4;

                int sectorIndex = vertexIndex / verticesPerSector;
                int indexInSector = vertexIndex % verticesPerSector;
                bool isNearVertex = (indexInSector & 2) == 0;
                BoundSector sector = Sectors[sectorIndex];
                Color color = Palette[sector.Pocket];
                Color fadedColor = new(1, 1, 1, color.a);
                Output[vertexIndex] = isNearVertex ? fadedColor : color;
            }
        }
    }
}