using Backstreets.Data;
using Backstreets.FOV;
using Backstreets.FOV.Builder;
using Backstreets.FOV.Geometry;
using Backstreets.FOV.Jobs;
using Backstreets.Pocket;
using Unity.Collections;
using Unity.Jobs;
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

            IGeometrySource geometrySource = new SceneGeometrySource(targetComponent.gameObject.scene);
            fovBuilder = new FieldOfViewBuilder(geometrySource);

            fovMesh = new Mesh();
            fovMesh.MarkDynamic();

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
            DestroyImmediate(fovMesh);
        }

        private void RegenerateMesh()
        {
            fovBuilder.SetOrigin(anchor.Position, pocket);
            using FieldOfView fov = fovBuilder.Build(Allocator.TempJob).Complete(); // TODO: Complete the job during repaint event
            using FanMeshData meshData = ScheduleMeshGeneration(fov).Complete();
            meshData.Apply(fovMesh);
            FanMeshColoring.SetColor(fovMesh, DefaultPalette);
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

        private static readonly Color Purple = new(0.5f, 0.25f, 1f); 
        private static FanMeshColoring.Palette DefaultPalette =>
            new FanMeshColoring.Palette
            {
                origin = Color.white,
                odd = Purple,
                even = Purple,
            }.Alpha(0.3f);

        private static JobPromise<FanMeshData> ScheduleMeshGeneration(FieldOfView fov)
        {
            FanMeshData meshData = new(fov.BoundsLength, Allocator.TempJob);
            NativeArray<BoundSector> sectors = fov.GetAllBoundSectors(Allocator.TempJob);
            BuildMeshDataJob job = new(sectors, fov.Space, meshData);
            JobHandle handle = job.Schedule();
            sectors.Dispose(handle);
            return new JobPromise<FanMeshData>(handle, meshData);
        }
    }
}