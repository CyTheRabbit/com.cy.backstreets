using System;
using UnityEngine;

namespace Backstreets.Viewport
{
    internal struct ViewportPoint : IComparable<ViewportPoint>
    {
        public ViewportPoint(Vector2 xy, float angle)
        {
            XY = xy;
            Angle = angle;
        }

        public ViewportPoint(Vector2 xy)
        {
            XY = xy;
            Angle = Vector2.SignedAngle(Vector2.right, xy);
        }

        public Vector2 XY;
        public float Angle;
        public float SqrDistance => XY.sqrMagnitude;

        public override string ToString() => $"({XY.x}:{XY.y})";
        public int CompareTo(ViewportPoint other) => CompareAngle(this, other);

        public static implicit operator Vector2(ViewportPoint point) => point.XY;
        public static bool operator <(ViewportPoint a, ViewportPoint b) => CompareAngle(a, b) < 0;
        public static bool operator >(ViewportPoint a, ViewportPoint b) => CompareAngle(a, b) > 0;
        public static bool operator <=(ViewportPoint a, ViewportPoint b) => CompareAngle(a, b) <= 0;
        public static bool operator >=(ViewportPoint a, ViewportPoint b) => CompareAngle(a, b) >= 0;

        public static int CompareAngle(ViewportPoint a, ViewportPoint b)
        {
            float angleDifference = b.Angle - a.Angle;
            float cyclicDifference = Mathf.Repeat(angleDifference, 360);
            return cyclicDifference switch
            {
                0 => 0,
                < 180 => 1,
                180 => 0,
                > 180 => -1,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}