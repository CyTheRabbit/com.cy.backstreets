using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Backstreets.FOV
{
    public struct FanMeshData : INativeDisposable
    {
        internal NativeList<float3> Vertices;
        internal NativeList<int> Indices;

        public FanMeshData(int lineCount, Allocator allocator)
        {
            Vertices = new NativeList<float3>(lineCount * 2 + 1, allocator);
            Indices = new NativeList<int>(lineCount * 3, allocator);
        }

        public void Apply(Mesh mesh, int submesh = 0)
        {
            mesh.Clear();
            mesh.SetVertices(Vertices.AsArray().Reinterpret<Vector3>());
            mesh.SetIndices(Indices.AsArray(), MeshTopology.Triangles, submesh);
        }

        public void Dispose()
        {
            Vertices.Dispose();
            Indices.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps) => JobHandle.CombineDependencies(
            Vertices.Dispose(inputDeps),
            Indices.Dispose(inputDeps));
    }
}