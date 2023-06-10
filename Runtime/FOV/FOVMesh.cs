using System;
using Backstreets.Data;
using Unity.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Backstreets.FOV
{
    public class FOVMesh : IDisposable
    {
        public readonly Mesh Mesh;
        public NativeList<PocketID> SubMeshIdentifiers;

        public FOVMesh()
        {
            Mesh = new Mesh();
            Mesh.MarkDynamic();

            SubMeshIdentifiers = new NativeList<PocketID>(initialCapacity: 16, Allocator.Persistent);
        }

        public int GetSubMeshIndex(PocketID pocket)
        {
            for (int i = 0; i < SubMeshIdentifiers.Length; i++)
            {
                if (SubMeshIdentifiers[i] == pocket) return i;
            }

            return -1;
        }

        public void Dispose()
        {
            Object.DestroyImmediate(Mesh);
            SubMeshIdentifiers.Dispose();
        }
    }
}
