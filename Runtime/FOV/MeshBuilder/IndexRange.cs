namespace Backstreets.FOV.MeshBuilder
{
    public struct IndexRange
    {
        public int Start;
        public int Length;

        public void Deconstruct(out int start, out int length)
        {
            start = Start;
            length = Length;
        }
    }
}