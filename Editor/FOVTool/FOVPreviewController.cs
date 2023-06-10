using System;
using System.Collections.Generic;
using Backstreets.Data;
using Backstreets.FOV;
using Backstreets.FOV.Builder;
using Backstreets.FOV.MeshBuilder;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Backstreets.Editor.FOVTool
{
    public class FOVPreviewController : IDisposable
    {
        private readonly FOVMesh fovMesh;
        private readonly SceneGeometrySource geometrySource;
        private readonly FieldOfViewBuilder fovBuilder;
        private readonly PocketID pocket;
        private readonly AnchorHandle anchor;
        private readonly FOVPreviewTool owner;
        private readonly BuildRequest buildRequestTemplate;

        public FOVPreviewController(Vector3 position, PocketID pocketID, Scene scene, FOVPreviewTool tool)
        {
            fovMesh = new FOVMesh();
            geometrySource = new SceneGeometrySource(scene);
            fovBuilder = new FieldOfViewBuilder(geometrySource);
            anchor = new AnchorHandle(position, tool.AnchorIcon);
            pocket = pocketID;
            owner = tool;

            buildRequestTemplate = new BuildRequest
            {
                Output = fovMesh,
                Mappings = new Dictionary<VertexAttribute, BuildRequest.AttributeType>
                {
                    [VertexAttribute.Position] = BuildRequest.AttributeType.WorldPosition,
                    [VertexAttribute.Normal] = BuildRequest.AttributeType.Normal,
                    [VertexAttribute.Color] = BuildRequest.AttributeType.PocketColor,
                    [VertexAttribute.TexCoord0] = BuildRequest.AttributeType.LocalPosition,
                },
                DebugPalette = geometrySource.DebugPalette,
            };

            RegenerateMesh();
        }

        public void Update()
        {
            if (UpdateHandles())
            {
                RegenerateMesh();
            }

            if (Event.current is { type: EventType.Repaint })
            {
                DrawMesh();
            }
        }

        public void Dispose()
        {
            geometrySource?.Dispose();
            fovMesh?.Dispose();
        }

        private bool UpdateHandles() => anchor.Update();

        private void RegenerateMesh()
        {
            Profiler.BeginSample("FOV Mesh Regeneration", owner);
            {
                fovBuilder.SetOrigin(anchor.Position, pocket);
                using FieldOfView fov = fovBuilder.Build(Allocator.TempJob).Complete(); // TODO: Complete the job during repaint event

                BuildRequest request = buildRequestTemplate;
                request.FieldOfView = fov;
                FOVMeshBuilder.Build(request);
            }
            Profiler.EndSample();
        }

        private void DrawMesh()
        {
            Color color = Handles.color * new Color(1f, 1f, 1f, Handles.lighting ? 1f : 0.5f);
            CommandBuffer cmd = CommandBufferPool.Get("Visibility Preview");
            cmd.SetGlobalColor("_HandleColor", color);
            cmd.SetGlobalFloat("_HandleSize", 1);
            cmd.DrawMesh(fovMesh.Mesh, Gizmos.matrix, HandleUtility.handleMaterial, 0, 0);
            Graphics.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
