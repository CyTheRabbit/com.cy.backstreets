using Backstreets.FOV.MeshBuilder.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Backstreets.FOV.MeshBuilder.Handlers
{
    internal static class IndicesHandler
    {
        public static void InitIndices(ref MeshBuildingContext context, ref Mesh.MeshData meshData)
        {
            meshData.SetIndexBufferParams(context.IndexCount, IndexFormat.UInt32);
            InitIndicesJob job = new() { Output = meshData.GetIndexData<QuadIndices>() };
            job.Run(context.Sectors.Length);
        }


        [BurstCompile]
        private struct InitIndicesJob : IJobParallelFor
        {
            [WriteOnly] public NativeArray<QuadIndices> Output;

            public void Execute(int index)
            {
                const int verticesPerSector = 4;
                Output[index] = new QuadIndices(first: index * verticesPerSector, 0, 1, 2, 3, 2, 1);
            }
        }
    }
}