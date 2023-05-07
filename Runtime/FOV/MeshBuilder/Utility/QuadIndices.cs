using System.Diagnostics.CodeAnalysis;

namespace Backstreets.FOV.MeshBuilder.Utility
{
    [SuppressMessage("ReSharper", "NotAccessedField.Local", 
        Justification = "Used only to produce multiple elements in a single iteration of a job.")]
    internal struct QuadIndices
    {
        public int A, B, C, D, E, F;

        public QuadIndices(int first, int a, int b, int c, int d, int e, int f)
        {
            A = first + a;
            B = first + b;
            C = first + c;
            D = first + d;
            E = first + e;
            F = first + f;
        }
    }
}