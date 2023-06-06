using System;
using System.Collections.Generic;
using Backstreets.FOV.MeshBuilder.Handlers;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Backstreets.FOV.MeshBuilder
{
    internal struct FOVMeshBuilder
    {
        public static void Build(BuildRequest request)
        {
            Profiler.BeginSample("Build FOV mesh");
            {
                FOVMeshBuilder builder = new(request);
                builder.InitIndices();
                builder.InitVertices();
                builder.InitSubMeshes();
                builder.Complete();
            }
            Profiler.EndSample();
        }


        private MeshBuildingContext context;
        private readonly Mesh.MeshDataArray meshDataArray;
        private Mesh.MeshData meshData;

        private FOVMeshBuilder(BuildRequest request)
        {
            ref FieldOfView fov = ref request.FieldOfView;
            OrderedBoundSectors sectors = new(ref fov, Allocator.TempJob);
            meshDataArray = Mesh.AllocateWritableMeshData(meshCount: 1);
            meshData = meshDataArray[0];

            context = new MeshBuildingContext(request, sectors);
        }

        private void InitIndices() => IndicesHandler.InitIndices(ref context, ref meshData);

        private void InitVertices()
        {
            DeclareVertexAttributes();
            PopulateAttributeData();
        }

        private void DeclareVertexAttributes()
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

        private void PopulateAttributeData()
        {
            Dictionary<VertexAttribute, BuildRequest.AttributeType> attributeMappings = context.Request.Mappings;
            foreach ((VertexAttribute output, BuildRequest.AttributeType attribute) in attributeMappings)
            {
                GetHandler(attribute).PopulateVertices(ref context, ref meshData, attribute, output);
            }
        }

        private void InitSubMeshes()
        {
            int pocketsCount = context.OrderedSectors.PocketIDs.Length;
            meshData.subMeshCount = 1 + pocketsCount;
            meshData.SetSubMesh(0, new SubMeshDescriptor(indexStart: 0, context.IndexCount));

            for (int i = 0; i < pocketsCount; i++)
            {
                const int indicesPerSector = 6;
                const int verticesPerSector = 4;
                IndexRange sectorsRange = context.OrderedSectors.Ranges[i];
                meshData.SetSubMesh(i + 1, new SubMeshDescriptor
                {
                    indexStart = sectorsRange.Start * indicesPerSector,
                    indexCount = sectorsRange.Length * indicesPerSector,
                    firstVertex = sectorsRange.Start * verticesPerSector,
                    vertexCount = sectorsRange.Length * verticesPerSector,
                });
            }
        }

        private void Complete()
        {
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, context.Request.Mesh);
            context.OrderedSectors.Dispose();
        }


        private static readonly Dictionary<BuildRequest.AttributeType, IAttributeHandler> AttributeHandlersCache = new()
        {
            { BuildRequest.AttributeType.Normal, new NormalHandler() },
            { BuildRequest.AttributeType.LocalPosition, new LocalPositionHandler() },
            { BuildRequest.AttributeType.PocketColor, new PocketColorHandler()},
            { BuildRequest.AttributeType.WorldPosition, new WorldPositionHandler() }
        };

        private static IAttributeHandler GetHandler(BuildRequest.AttributeType attribute) => 
            AttributeHandlersCache[attribute] ?? throw new ArgumentOutOfRangeException();
    }
}
