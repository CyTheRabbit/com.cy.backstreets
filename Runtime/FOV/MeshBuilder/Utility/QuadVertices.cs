using Unity.Mathematics;

namespace Backstreets.FOV.MeshBuilder.Utility
{
    internal struct QuadVertices
    {
        // ReSharper disable NotAccessedField.Local
        public float3 A, B, C, D;
        // ReSharper restore NotAccessedField.Local

        public QuadVertices(float3 a, float3 b, float3 c, float3 d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }
    }
}