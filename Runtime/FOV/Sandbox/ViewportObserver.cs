using System;
using System.Linq;
using Backstreets.FOV.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Backstreets.FOV.Sandbox
{
    public class ViewportObserver : MonoBehaviour
    {
        [SerializeField] private ViewportObstacle[] obstacles = Array.Empty<ViewportObstacle>();
        [SerializeField] private FanMeshColoring.Palette palette = DefaultPalette;
        private Mesh mesh;

        public void OnDrawGizmos()
        {
            RebuildFieldOfViewMesh();
            RenderMesh(mesh);
        }

        private void RebuildFieldOfViewMesh()
        {
            float2 origin = ((float3)transform.position).xy;
            int totalVertexCount = obstacles.Sum(obstacle => obstacle.Vertices.Length);
            float2[][] shapes = obstacles.Select(obstacle => obstacle.Vertices).ToArray();

            using JobPromise<FieldOfView> fieldOfView = FieldOfViewBuilder.Build(origin, shapes);
            using JobPromise<FanMeshData> buildMesh = ScheduleMeshGeneration(fieldOfView, totalVertexCount);

            FanMeshData meshData = buildMesh.Complete();

            ReinitializeMesh();
            meshData.Apply(mesh);
            FanMeshColoring.SetColor(mesh, palette);
        }

        private void ReinitializeMesh()
        {
            if (mesh != null) return;
            mesh = new Mesh();
            mesh.MarkDynamic();
        }


        private static JobPromise<FanMeshData> ScheduleMeshGeneration(
            in JobPromise<FieldOfView> fieldOfView,
            int vertexCount)
        {
            FanMeshData meshData = new(vertexCount, Allocator.TempJob);
            BuildTriangleFanJob job = new(fieldOfView.Result, meshData);
            return new JobPromise<FanMeshData>(job.Schedule(fieldOfView.Handle), meshData);
        }

        private static void RenderMesh(Mesh mesh)
        {
            Color color = Handles.color * new Color(1f, 1f, 1f, 0.5f) + (Handles.lighting
                ? new Color(0.0f, 0.0f, 0.0f, 0.5f)
                : new Color(0.0f, 0.0f, 0.0f, 0.0f));
            CommandBuffer cmd = CommandBufferPool.Get("Field of view");
            cmd.SetGlobalColor("_HandleColor", color);
            cmd.SetGlobalFloat("_HandleSize", 1);
            cmd.DrawMesh(mesh, Gizmos.matrix, HandleUtility.handleMaterial, 0, 0);
            Graphics.ExecuteCommandBuffer(cmd);
        }

        private static FanMeshColoring.Palette DefaultPalette =>
            new FanMeshColoring.Palette
            {
                origin = Color.white,
                odd = Color.magenta,
                even = Color.cyan,
            }.Alpha(0.02f);
    }
}