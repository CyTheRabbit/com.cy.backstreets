using System;
using UnityEngine;

namespace Backstreets.Viewport
{
    internal static class ViewportMath
    {
        internal static float Determinant(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        internal static Vector2? GetIntersection(Vector2 aLeft, Vector2 aRight, Vector2 bLeft, Vector2 bRight)
        {
            Vector2 aDiff = aLeft - aRight;
            Vector2 bDiff = bLeft - bRight;
            float denominator = Determinant(aDiff, bDiff);
            if (denominator == 0) return null; // TODO: test if if-less is faster
            float aDeterminant = Determinant(aLeft, aRight);
            float bDeterminant = Determinant(bLeft, bRight);
            return new Vector2(
                x: (bDiff.x * aDeterminant - aDiff.x * bDeterminant) / denominator,
                y: (bDiff.y * aDeterminant - aDiff.y * bDeterminant) / denominator);
        }

        internal static Vector2? ProjectFromOrigin(Vector2 left, Vector2 right, Vector2 rayFromOrigin)
        {
            float x1 = left.x, x2 = right.x, x3 = rayFromOrigin.x;
            float y1 = left.y, y2 = right.y, y3 = rayFromOrigin.y;
            float denominator = x3 * (y1 - y2) - y3 * (x1 - x2);
            // Vector2 lineDiff = left - right;
            // float denominator = Determinant(lineDiff, rayFromOrigin);
            if (denominator == 0) return Vector2.zero;
            float distance = -Determinant(left, right) / denominator;
            return rayFromOrigin * distance;
        }

        internal static RelativeDomain GetDomain(Vector2 left, Vector2 right, Vector2 testPoint)
        {
            Vector2 tangent = right - left;
            Vector2 normal = Vector2.Perpendicular(tangent);
            Vector2 toTestPoint = testPoint - left;
            return Vector2.Dot(normal, toTestPoint) switch
            {
                < 0 => RelativeDomain.Bottom,
                0 => RelativeDomain.Line,
                > 0 => RelativeDomain.Top,
                _ => throw new ArithmeticException()
            };
        }

        internal static RelativeDomain GetOriginDomain(Vector2 left, Vector2 right) =>
            // If you take GetDomain method and substitute testPoint with Vector2.zero,
            // in the end you get the following expression:
            Determinant(left, right) switch
            {
                < 0 => RelativeDomain.Bottom,
                0 => RelativeDomain.Line,
                > 0 => RelativeDomain.Top,
                _ => throw new ArithmeticException()
            };

        internal static RelativeDomain Combine(RelativeDomain x, RelativeDomain y) => x | y;
        
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

        internal static float? GetDistanceFromOrigin(ViewportLine line, float angle)
        {
            float x1 = line.Left.XY.x, x2 = line.Right.XY.x;
            float y1 = line.Left.XY.y, y2 = line.Right.XY.y;
            float denominator = Mathf.Cos(angle) * (y1 - y2) - Mathf.Sin(angle) * (x1 - x2);
            if (denominator == 0) return null;
            float determinant = x1 * y2 - y1 * x2;
            float distance = - determinant / denominator;
            return distance;
        }


        /// <summary>
        /// Since <see cref="ViewportMath"/> uses left-right notation for line points, to define 
        /// </summary>
        [Flags]
        internal enum RelativeDomain
        {
            Line = 0b00,
            Top = 0b01,
            Bottom = 0b10,
            Both = 0b11,
        }
    }
}