using System;
using System.Collections.Generic;
using Backstreets.FOV.Geometry;
using Backstreets.FOV.MeshBuilder.Handlers;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Backstreets.FOV.MeshBuilder
{
    internal struct FOVMeshBuilder
    {
        private MeshBuildingContext context;
        private readonly Mesh.MeshDataArray meshDataArray;
        private Mesh.MeshData meshData;

        public FOVMeshBuilder(BuildRequest request)
        {
            ref FieldOfView fov = ref request.FieldOfView;
            NativeArray<BoundSector> sectors = fov.GetAllBoundSectors(Allocator.TempJob);
            meshDataArray = Mesh.AllocateWritableMeshData(meshCount: 1);
            meshData = meshDataArray[0];

            context = new MeshBuildingContext(request, sectors);
        }

        public void InitIndices() => IndicesHandler.InitIndices(ref context, ref meshData);

        public void InitVertices()
        {
            DeclareVertexAttributes();
            PopulateAttributeData();
        }

        public void DeclareVertexAttributes()
        {
            Dictionary<VertexAttribute, BuildRequest.AttributeType> attributeMappings = context.Request.Mappings;
            NativeList<VertexAttributeDescriptor> vertexBufferParams = new(attributeMappings.Count, Allocator.Temp);
            foreach ((VertexAttribute output, BuildRequest.AttributeType attribute) in attributeMappings)
            {
                VertexAttributeDescriptor descriptor = GetHandler(attribute).MakeDescriptor(attribute, output, vertexBufferParams.Length);
                vertexBufferParams.AddNoResize(descriptor);
            }

            meshData.SetVertexBufferParams(context.VertexCount, vertexBufferParams.AsArray());
            vertexBufferParams.Dispose();
        }

        public void PopulateAttributeData()
        {
            Dictionary<VertexAttribute, BuildRequest.AttributeType> attributeMappings = context.Request.Mappings;
            foreach ((VertexAttribute output, BuildRequest.AttributeType attribute) in attributeMappings)
            {
                GetHandler(attribute).PopulateVertices(ref context, ref meshData, attribute, output);
            }
        }

        public void InitSubMeshes()
        {
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(indexStart: 0, context.IndexCount));
        }

        public void Complete()
        {
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, context.Request.Mesh);
            context.Sectors.Dispose();
        }


        private static readonly Dictionary<BuildRequest.AttributeType, IAttributeHandler> AttributeHandlers = new()
        {
            { BuildRequest.AttributeType.Normal, new NormalHandler() },
            { BuildRequest.AttributeType.LocalPosition, new LocalPositionHandler() },
            { BuildRequest.AttributeType.PocketColor, new PocketColorHandler()},
            { BuildRequest.AttributeType.WorldPosition, new WorldPositionHandler() }
        };

        public static void BuildMesh(BuildRequest request)
        {
            FOVMeshBuilder builder = new(request);

            builder.InitIndices();
            builder.InitVertices();
            builder.InitSubMeshes();
            builder.Complete();
        }

        private static IAttributeHandler GetHandler(BuildRequest.AttributeType attribute) => 
            AttributeHandlers[attribute] ?? throw new ArgumentOutOfRangeException();
    }
}