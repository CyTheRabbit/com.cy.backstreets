using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Backstreets.Data
{
    [Serializable]
    public struct PortalData
    {
        public Vector2 position;
        public float rotation;
        [FormerlySerializedAs("scale")] public float width;

        public Matrix4x4 LocalToWorld =>
            Matrix4x4.TRS(
                pos: position, 
                q: Quaternion.AngleAxis(rotation, Vector3.forward), 
                s: Vector3.one);

        public static PortalData Default => new() {position = Vector2.zero, rotation = 0, width = 1};
    }
}