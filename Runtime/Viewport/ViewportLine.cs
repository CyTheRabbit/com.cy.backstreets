namespace Backstreets.Viewport
{
    internal struct ViewportLine
    {
        public ViewportLine(ViewportPoint left, ViewportPoint right)
        {
            Left = left;
            Right = right;
        }

        public ViewportPoint Left;
        public ViewportPoint Right;

        public override string ToString() => $"{Left}-{Right}";

        public int CompareTo(ViewportLine other)
        {
            int leftComparison = Left.CompareTo(other.Left);
            return leftComparison != 0 ? leftComparison 
                : Right.CompareTo(other.Right);
        }
    }
}