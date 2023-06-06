using System.Collections.Generic;
using Backstreets.Data;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Backstreets.FOV.MeshBuilder
{
    public struct BuildRequest
    {
        public FieldOfView FieldOfView;
        public Mesh Mesh;
        public Dictionary<VertexAttribute, AttributeType> Mappings;
        public PocketID? Filter;

        public NativeParallelHashMap<PocketID, Color>? DebugPalette;


        public enum AttributeType
        {
            WorldPosition,
            LocalPosition,
            PocketColor,
            Normal,
        }
    }
}
