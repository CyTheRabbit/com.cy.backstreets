using System;
using UnityEngine;

namespace Backstreets.Viewport
{
    internal struct ViewportLine : IComparable<ViewportLine>
    {
        public ViewportLine(ViewportPoint left, ViewportPoint right)
        {
            Left = left;
            Right = right;
        }

        public ViewportPoint Left;
        public ViewportPoint Right;

        public Vector2 Tangent => (Vector2)Right - Left;
        public Vector2 Normal => -Vector2.Perpendicular(Tangent);

        public ViewportLine NextLeft(ViewportPoint nextCorner) => new() { Left = nextCorner, Right = Left };
        public ViewportLine NextRight(ViewportPoint nextCorner) => new() { Left = Right, Right = nextCorner };
        public override string ToString() => $"{Left}-{Right}";

        public bool IsFacingOrigin()
        {
            float angleDifference = Right.Angle - Left.Angle;
            float cyclicDifference = Mathf.Repeat(angleDifference, 360);
            return cyclicDifference < 180;
        }

        public int CompareTo(ViewportLine other)
        {
            int leftComparison = Left.CompareTo(other.Left);
            return leftComparison != 0 ? leftComparison 
                : Right.CompareTo(other.Right);
        }
    }
}