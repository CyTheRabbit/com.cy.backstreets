using System;
using System.Collections.Generic;
using UnityEngine;

namespace Backstreets.Viewport
{
    public struct Corner
    {
        public Vector2 Position;
        public Vector2 Left;
        public Vector2 Right;
        public float Angle; // TODO: Test if tangent suits better than angle


        public static RelativeDirection GetRelativeDirection(Vector2 position, Vector2 other) =>
            Vector2.Dot(Vector2.Perpendicular(position), other) switch
            {
                < 0 => RelativeDirection.Left,
                0 => RelativeDirection.Straight,
                > 0 => RelativeDirection.Right,
                _ => throw new ArithmeticException()
            };

        public enum RelativeDirection
        {
            Straight,
            Left,
            Right,
        }

        public readonly struct CompareByAngle : IComparer<Corner>
        {
            public int Compare(Corner x, Corner y) => 
                x.Angle.CompareTo(y.Angle);
        }

        public readonly struct CompareByAngleCyclic : IComparer<Corner>
        {
            public int Compare(Corner x, Corner y) => 
                ViewportMath.CompareAngleCyclic(x.Angle, y.Angle);
        }
    }
}