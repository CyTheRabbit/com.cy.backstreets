using Unity.Mathematics;

namespace Backstreets.FOV.Geometry
{
    public struct Bound
    {
        public Line Line;
        public int EdgeIndex;

        public Bound(Line line, int edgeIndex)
        {
            Line = line;
            EdgeIndex = edgeIndex;
        }

        public float2 Right
        {
            get => Line.Right;
            set => Line.Right = value;
        }

        public float2 Left
        {
            get => Line.Left;
            set => Line.Left = value;
        }

        public static implicit operator Line(Bound bound) => bound.Line;
    }
}