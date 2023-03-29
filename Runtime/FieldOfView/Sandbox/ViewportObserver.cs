using System;
using System.Linq;
using Backstreets.Viewport;
using Backstreets.Viewport.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Backstreets
{
    public class ViewportObserver : MonoBehaviour
    {
        [SerializeField] private ViewportObstacle[] obstacles = Array.Empty<ViewportObstacle>();
        [SerializeField] private Color fieldOfViewColor = new(0.5f, 1, 0.75f, 0.02f);
        [SerializeField] private ColorMode colorMode;
        private Mesh mesh;

        public void OnDrawGizmos()
        {
            RebuildFieldOfViewMesh();
            RenderFieldOfView();
        }

        private void RebuildFieldOfViewMesh()
        {
            Transform transformCached = transform;
            ViewportSpace space = new(
                worldToViewport: transformCached.worldToLocalMatrix,
                viewportToWorld: transformCached.localToWorldMatrix);

            int totalVertexCount = obstacles.Sum(obstacle => obstacle.Vertices.Length);
            NativeArray<Corner> corners = new(totalVertexCount, Allocator.TempJob);
            NativeList<ViewportSegment> viewportSegments = new(totalVertexCount + 1, Allocator.TempJob);
            NativeList<Vector3> outputVertices = new(totalVertexCount * 2 + 1, Allocator.TempJob);
            NativeList<int> outputIndices = new(totalVertexCount * 3, Allocator.TempJob);

            JobHandle dataAssemblyHandle = ScheduleDataAssembly(in corners, space);
            JobHandle calculationHandle = ScheduleFieldOfViewCalculation(in corners, in viewportSegments, dataAssemblyHandle);
            JobHandle generateMeshHandle = ScheduleMeshGeneration(space, in viewportSegments, in outputVertices,
                in outputIndices, calculationHandle);

            JobHandle cleanupHandle = JobHandle.CombineDependencies(
                corners.Dispose(calculationHandle),
                viewportSegments.Dispose(generateMeshHandle));

            cleanupHandle.Complete();

            PrepareFieldOfViewMesh(outputVertices, outputIndices);

            outputVertices.Dispose();
            outputIndices.Dispose();
        }

        private JobHandle ScheduleDataAssembly(in NativeArray<Corner> corners, ViewportSpace space)
        {
            int totalVertexCount = obstacles.Sum(obstacle => obstacle.Vertices.Length);
            NativeArray<Vector2> source = new(totalVertexCount, Allocator.TempJob);
            NativeArray<int2> spans = new(obstacles.Length, Allocator.TempJob);

            int nextUnusedOutput = 0;
            for (int i = 0; i < obstacles.Length; i++)
            {
                Vector2[] obstacleVertices = obstacles[i].Vertices;
                int2 span = new(nextUnusedOutput, obstacleVertices.Length);
                source.Slice(span.x, span.y).CopyFrom(obstacleVertices);

                spans[i] = span;
                nextUnusedOutput += span.y;
            }

            JobHandle assemble = new BuildCornersJob(space, source, corners, spans).Schedule(arrayLength: spans.Length, innerloopBatchCount: 4);

            { // Cleanup
                source.Dispose(assemble);
                spans.Dispose(assemble);
            }

            return assemble;
        }

        private static JobHandle ScheduleFieldOfViewCalculation(
            in NativeArray<Corner> corners,
            in NativeList<ViewportSegment> fieldOfView,
            JobHandle inputDependency)
        {
            NativeArray<Corner> orderedCorners = new(corners.Length, Allocator.TempJob);
            LineOfSight lineOfSight = new(capacity: 16);

            JobHandle copyCorners = new CopyArrayJob<Corner>(corners, orderedCorners).Schedule(inputDependency);
            JobHandle orderCorners = orderedCorners.SortJob(new Corner.CompareByAngle()).Schedule(copyCorners);
            JobHandle raycastStartingLineOfSight =
                new RaycastLinesJob(corners, Vector2.left, lineOfSight).Schedule(inputDependency);
            JobHandle preparationJobs = JobHandle.CombineDependencies(raycastStartingLineOfSight, orderCorners);

            JobHandle buildSegments =
                new BuildViewportSegmentsJob(lineOfSight, orderedCorners, fieldOfView).Schedule(
                    preparationJobs);

            orderedCorners.Dispose(buildSegments);
            lineOfSight.Dispose(buildSegments);
            
            return buildSegments;
        }

        private static JobHandle ScheduleMeshGeneration(
            ViewportSpace space,
            in NativeList<ViewportSegment> segments,
            in NativeList<Vector3> outputVertices,
            in NativeList<int> outputIndices,
            JobHandle inputDependency)
        {
            BuildTriangleFanJob job = new(segments, space, outputVertices, outputIndices);
            return job.Schedule(inputDependency);
        }

        private void PrepareFieldOfViewMesh(NativeList<Vector3> outputVertices, NativeList<int> outputIndices)
        {
            mesh ??= new Mesh();
            mesh.Clear();
            mesh.SetVertices(outputVertices.AsArray());
            mesh.SetIndices(outputIndices.AsArray(), MeshTopology.Triangles, 0);

            PrepareMeshColor(outputVertices);
        }

        private void PrepareMeshColor(NativeList<Vector3> outputVertices)
        {
            NativeArray<Color> colors = new(outputVertices.AsArray().Length, Allocator.Temp);
            switch (colorMode)
            {
                case ColorMode.Straight:
                    FillArray(colors, fieldOfViewColor);
                    colors[0] = new Color(1, 1, 1, fieldOfViewColor.a);
                    break;
                case ColorMode.Triangles:
                    FillFanColors(colors, Color.white, Color.blue, Color.red, 0.1f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            mesh.SetColors(colors);
            colors.Dispose();
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

        private static void FillArray<T>(NativeArray<T> colors, T value) where T : struct
        {
            for (int i = 0; i < colors.Length; i++) colors[i] = value;
        }

        private static void FillFanColors(NativeArray<Color> colors, Color origin, Color a, Color b, float alpha)
        {
            a.a *= alpha;
            b.a *= alpha;
            origin.a *= alpha;
            colors[0] = origin;
            for (int i = 1; i < colors.Length; i++)
            {
                bool isOdd = (((i - 1) / 2) & 1) == 0;
                colors[i] = isOdd ? a : b;
            }
        }

        private enum ColorMode
        {
            Straight,
            Triangles,
        }
    }
}