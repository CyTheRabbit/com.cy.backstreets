using System;
using UnityEngine;

namespace Backstreets.FieldOfView
{
    public struct Line : IEquatable<Line>
    {
        public Vector2 Left;
        public Vector2 Right;

        public Line(Vector2 left, Vector2 right)
        {
            Left = left;
            Right = right;
        }

        public bool Equals(Line other) => Left.Equals(other.Left) && Right.Equals(other.Right);
        public override bool Equals(object obj) => obj is Line other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Left, Right);
    }
}