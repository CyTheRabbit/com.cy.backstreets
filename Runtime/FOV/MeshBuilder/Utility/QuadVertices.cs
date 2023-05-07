using System.Diagnostics.CodeAnalysis;
using Unity.Mathematics;

namespace Backstreets.FOV.MeshBuilder.Utility
{
    [SuppressMessage("ReSharper", "NotAccessedField.Local",
        Justification = "Used only to produce multiple elements in a single iteration of a job.")]
    internal struct QuadVertices
    {
        public float3 A, B, C, D;

        public QuadVertices(float3 a, float3 b, float3 c, float3 d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }
    }
}