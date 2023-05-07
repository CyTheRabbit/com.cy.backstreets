using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Backstreets.FOV.MeshBuilder.Handlers
{
    internal class NormalHandler : IAttributeHandler
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
            InitNormalsJob job = new() { Output = meshData.GetVertexData<float3>(stream) };
            job.Run(context.VertexCount);
        }


        private struct InitNormalsJob : IJobParallelFor
        {
            [WriteOnly] public NativeArray<float3> Output;

            public void Execute(int index)
            {
                Output[index] = new float3(0, 0, -1);
            }
        }
    }
}