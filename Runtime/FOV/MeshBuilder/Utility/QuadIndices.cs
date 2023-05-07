namespace Backstreets.FOV.MeshBuilder.Utility
{
    internal struct QuadIndices
    {
        // ReSharper disable NotAccessedField.Local
        public int A, B, C, D, E, F;
        // ReSharper restore NotAccessedField.Local

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