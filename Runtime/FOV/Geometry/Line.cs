using System;
using System.Diagnostics;
using Unity.Mathematics;

namespace Backstreets.FOV.Geometry
{
    [DebuggerDisplay("({Right.x}};{Right.y})—({Left.x};{Left.y})")]
    public struct Line : IEquatable<Line>
    {
        public float2 Right;
        public float2 Left;

        public Line(float2 right, float2 left)
        {
            Right = right;
            Left = left;
        }

        public Line Reverse() => new(Left, Right);

        public bool Equals(Line other) => Left.Equals(other.Left) && Right.Equals(other.Right);
        public override bool Equals(object obj) => obj is Line other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Left, Right);
    }
}