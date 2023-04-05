namespace Backstreets.FOV.Utility
{
    public struct IndexRange
    {
        public IndexRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        public int Start;
        public int End;
        public int Length => End - Start;
    }
}