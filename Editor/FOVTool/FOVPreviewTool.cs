using System.Collections.Generic;
using Backstreets.Data;
using Backstreets.FOV;
using Backstreets.FOV.Builder;
using Backstreets.FOV.MeshBuilder;
using Backstreets.Pocket;
using Unity.Collections;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Backstreets.Editor.FOVTool
{
    [EditorTool("Visibility Preview", componentToolTarget: typeof(PocketPrefabDetails))]
    public class FOVPreviewTool : EditorTool
    {
        [SerializeField] private Texture2D anchorIcon;
        private FieldOfViewBuilder fovBuilder;
        private Mesh fovMesh;
        private PocketID pocket;
        private AnchorHandle anchor;
        private GUIContent icon;
        private SceneGeometrySource geometrySource;
        private BuildRequest buildRequestTemplate;

        public override GUIContent toolbarIcon => icon;

        private void OnEnable()
        {
            icon = new GUIContent(anchorIcon, "Visibility Preview");
        }

        public override void OnActivated()
        {
            PocketPrefabDetails targetComponent = (PocketPrefabDetails)target;
            Vector3 anchorPosition = targetComponent.transform.position;

            pocket = targetComponent.PocketID;
            anchor = new AnchorHandle(anchorPosition, icon);

            geometrySource = new SceneGeometrySource(targetComponent.gameObject.scene);
            fovBuilder = new FieldOfViewBuilder(geometrySource);

            fovMesh = new Mesh();
            fovMesh.MarkDynamic();

            buildRequestTemplate = new BuildRequest
            {
                Mesh = fovMesh,
                Mappings = new Dictionary<VertexAttribute, BuildRequest.AttributeType>
                {
                    [VertexAttribute.Position] = BuildRequest.AttributeType.WorldPosition,
                    [VertexAttribute.Normal] = BuildRequest.AttributeType.Normal,
                    [VertexAttribute.Color] =  BuildRequest.AttributeType.PocketColor,
                    [VertexAttribute.TexCoord0] = BuildRequest.AttributeType.LocalPosition,
                },
                DebugPalette = geometrySource.DebugPalette,
            };

            RegenerateMesh();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (Event.current is {type: EventType.Repaint})
            {
                DrawMesh();
            }

            if (anchor.Update())
            {
                Profiler.BeginSample("FOV Mesh Regeneration", this);
                RegenerateMesh();
                Profiler.EndSample();
            }
        }

        public override void OnWillBeDeactivated()
        {
            geometrySource.Dispose();
            DestroyImmediate(fovMesh);
        }

        private void RegenerateMesh()
        {
            fovBuilder.SetOrigin(anchor.Position, pocket);
            using FieldOfView fov = fovBuilder.Build(Allocator.TempJob).Complete(); // TODO: Complete the job during repaint event

            BuildRequest request = buildRequestTemplate;
            request.FieldOfView = fov;
            FOVMeshBuilder.BuildMesh(request);
        }

        private void DrawMesh()
        {
            Color color = Handles.color * new Color(1f, 1f, 1f, Handles.lighting ? 1f : 0.5f);
            CommandBuffer cmd = CommandBufferPool.Get("Visibility Preview");
            cmd.SetGlobalColor("_HandleColor", color);
            cmd.SetGlobalFloat("_HandleSize", 1);
            cmd.DrawMesh(fovMesh, Gizmos.matrix, HandleUtility.handleMaterial, 0, 0);
            Graphics.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}