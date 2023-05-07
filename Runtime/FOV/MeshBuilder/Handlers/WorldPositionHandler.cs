using Backstreets.FOV.Geometry;
using Backstreets.FOV.MeshBuilder.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Backstreets.FOV.MeshBuilder.Handlers
{
    internal class WorldPositionHandler : IAttributeHandler
    {
        public VertexAttributeDescriptor MakeDescriptor(
            BuildRequest.AttributeType attribute,
            VertexAttribute output,
            int index) =>
            new(output, stream: index);

        public void PopulateVertices(
            ref MeshBuildingContext context,
            ref Mesh.MeshData meshData,
            BuildRequest.AttributeType attribute,
            VertexAttribute output)
        {
            int stream = meshData.GetVertexAttributeStream(output);
            InitVerticesJob job = new()
            {
                Sectors = context.Sectors,
                Space = context.Space,
                Output = meshData.GetVertexData<QuadVertices>(stream)
            };
            job.Run(context.Sectors.Length);
        }


        [BurstCompile]
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
    }
}