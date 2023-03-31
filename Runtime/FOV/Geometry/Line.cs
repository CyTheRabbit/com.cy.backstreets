using System;
using Unity.Mathematics;

namespace Backstreets.FOV.Geometry
{
    public struct Line : IEquatable<Line>
    {
        public float2 Left;
        public float2 Right;

        public Line(float2 left, float2 right)
        {
            Left = left;
            Right = right;
        }

        public bool Equals(Line other) => Left.Equals(other.Left) && Right.Equals(other.Right);
        public override bool Equals(object obj) => obj is Line other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Left, Right);
    }
}