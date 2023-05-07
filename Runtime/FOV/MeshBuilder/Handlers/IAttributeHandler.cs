using UnityEngine.Rendering;

namespace Backstreets.FOV.MeshBuilder.Handlers
{
    internal interface IAttributeHandler
    {
        VertexAttributeDescriptor MakeDescriptor(BuildRequest.AttributeType attribute, VertexAttribute output, int index);

        void PopulateVertices(ref MeshBuildingContext context, ref UnityEngine.Mesh.MeshData meshData, BuildRequest.AttributeType attribute, VertexAttribute output);
    }
}