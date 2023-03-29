using System;
using UnityEngine;

namespace Backstreets.FieldOfView
{
    internal static class LineMath
    {
        private static float Determinant(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        internal static Vector2? GetIntersection(Line a, Line b)
        {
            Vector2 aDiff = a.Left - a.Right;
            Vector2 bDiff = b.Left - b.Right;
            float denominator = Determinant(aDiff, bDiff);
            if (denominator == 0) return null; // TODO: test if if-less is faster
            float aDeterminant = Determinant(a.Left, a.Right);
            float bDeterminant = Determinant(b.Left, b.Right);
            return (bDiff * aDeterminant - aDiff * bDeterminant) / denominator;
        }

        internal static Vector2? ProjectFromOrigin(Line line, Vector2 ray)
        {
            Vector2 lineDiff = line.Left - line.Right;
            float denominator = Determinant(lineDiff, ray);
            if (denominator == 0) return Vector2.zero;
            float distance = Determinant(line.Left, line.Right) / denominator;
            return ray * distance;
        }

        internal static LineDomain GetDomain(Line line, Vector2 testPoint)
        {
            // TODO: Replace vector math with determinant of line relative to testPoint
            Vector2 tangent = line.Right - line.Left;
            Vector2 normal = Vector2.Perpendicular(tangent);
            Vector2 toTestPoint = testPoint - line.Left;
            return Vector2.Dot(normal, toTestPoint) switch
            {
                < 0 => LineDomain.Bottom,
                0 => LineDomain.Line,
                > 0 => LineDomain.Top,
                _ => throw new ArithmeticException()
            };
        }

        internal static RayDomain GetDomain(Vector2 ray, Vector2 testPoint) =>
            Vector2.Dot(Vector2.Perpendicular(ray), testPoint) switch
            {
                < 0 => RayDomain.Left,
                0 => RayDomain.Straight,
                > 0 => RayDomain.Right,
                _ => throw new ArithmeticException()
            };

        internal static LineDomain GetOriginDomain(Line line) =>
            // If you take GetDomain method and substitute testPoint with Vector2.zero,
            // in the end you get the following expression:
            Determinant(line.Left, line.Right) switch
            {
                < 0 => LineDomain.Bottom,
                0 => LineDomain.Line,
                > 0 => LineDomain.Top,
                _ => throw new ArithmeticException()
            };

        internal static LineDomain Combine(LineDomain x, LineDomain y) => x | y;

        internal static int CompareAngleCyclic(float x, float y)
        {
            float cyclicDifference = Mathf.Repeat(x - y, 360);
            return cyclicDifference switch
            {
                0 => 0,
                < 180 => 1,
                180 => 0,
                > 180 => -1,
                _ => throw new ArithmeticException(),
            };
        }

        internal static float? GetDistanceFromOrigin(Line line, Vector2 ray)
        {
            Vector2 lineDiff = line.Left - line.Right;
            float denominator = Determinant(ray, lineDiff);
            if (denominator == 0) return null;
            float determinant = Determinant(line.Left, line.Right);
            float distance = -determinant / denominator;
            return distance;
        }


        /// <summary>
        /// Since <see cref="Line"/> uses left-right notation, to define domains relative to the line
        /// we use top-bottom notation.
        /// </summary>
        [Flags]
        internal enum LineDomain
        {
            Line = 0b00,
            Top = 0b01,
            Bottom = 0b10,
            Both = 0b11,
        }

        internal enum RayDomain
        {
            Straight,
            Left,
            Right,
        }
    }
}