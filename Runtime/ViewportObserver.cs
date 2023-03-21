using Backstreets.Viewport.Jobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Backstreets
{
    public class ViewportObserver : MonoBehaviour
    {
        [SerializeField] private Vector2[] vertices;
        [SerializeField] private Color fieldOfViewColor = new(0.5f, 1, 0.75f, 0.02f);
        private Mesh mesh;

        public void OnDrawGizmos()
        {
            if (vertices.Length <= 2) return;

            PortalWindow window = new(transform.position, new Vector2(-10, 10), new Vector2(10, 10));
            ViewportSpace space = new(window);

            using NativeArray<Vector2> verticesArray = new(vertices, Allocator.TempJob);
            using NativeArray<ViewportLine> edges = new(vertices.Length, Allocator.TempJob);
            using NativeList<ViewportLine> visibleEdges = new(vertices.Length * 2, Allocator.TempJob);
            using NativeList<Vector3> outputVertices = new(0, Allocator.TempJob);
            using NativeList<int> outputIndices = new(0, Allocator.TempJob);

            {
                JobHandle buildEdges = new BuildEdgesJob(verticesArray, space, edges).Schedule();
                JobHandle sortEdges = edges.SortJob().Schedule(buildEdges);
                JobHandle getVisibleEdges = new GetVisibleEdgesJob(edges, visibleEdges).Schedule(sortEdges);
                JobHandle outputFan = new BuildTriangleFanJob(visibleEdges, space, outputVertices, outputIndices).Schedule(getVisibleEdges);
                outputFan.Complete();
            }

            DrawEdgeGizmos(edges, space);
            PrepareFieldOfViewMesh(outputVertices, outputIndices);
            RenderFieldOfView();
        }

        private void RenderFieldOfView()
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

        private void PrepareFieldOfViewMesh(NativeList<Vector3> outputVertices, NativeList<int> outputIndices)
        {
            mesh ??= new Mesh();
            mesh.SetVertices(outputVertices.AsArray());
            mesh.SetIndices(outputIndices.AsArray(), MeshTopology.Triangles, 0);

            NativeArray<Color> colors = new(outputVertices.Length, Allocator.Temp);
            for (int i = 0; i < colors.Length; i++) colors[i] = fieldOfViewColor;
            mesh.SetColors(colors);
            colors.Dispose();
        }

        private static void DrawEdgeGizmos(NativeArray<ViewportLine> edges, ViewportSpace space)
        {
            Handles.color = Color.white;
            foreach (ViewportLine edge in edges)
            {
                Handles.DrawLine(space.Convert(edge.Left), space.Convert(edge.Right), 2);
            }
        }
    }
}