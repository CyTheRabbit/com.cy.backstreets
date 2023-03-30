using System;
using Unity.Mathematics;

namespace Backstreets.FieldOfView
{
    internal static class LineMath
    {
        private static float Determinant(float2 a, float2 b) => a.x * b.y - a.y * b.x;

        internal static float Angle(float2 point)
        {
            float distance = math.length(point);
            float sign = math.sign(point.y) switch { 0 when point.x < 0 => 1, var rawSign => rawSign };
            return distance < math.EPSILON ? 0 : math.degrees(math.acos(point.x / distance)) * sign;
        }

        internal static float2? GetIntersection(Line a, Line b)
        {
            float2 aDiff = a.Left - a.Right;
            float2 bDiff = b.Left - b.Right;
            float denominator = Determinant(aDiff, bDiff);
            if (denominator == 0) return null; // TODO: test if if-less is faster
            float aDeterminant = Determinant(a.Left, a.Right);
            float bDeterminant = Determinant(b.Left, b.Right);
            return (bDiff * aDeterminant - aDiff * bDeterminant) / denominator;
        }

        internal static float2? ProjectFromOrigin(Line line, float2 ray)
        {
            float2 lineDiff = line.Left - line.Right;
            float denominator = Determinant(lineDiff, ray);
            if (denominator == 0) return float2.zero;
            float distance = Determinant(line.Left, line.Right) / denominator;
            return ray * distance;
        }

        internal static LineDomain GetDomain(Line line, float2 testPoint) => 
            Determinant(line.Left - line.Right, line.Left - testPoint) switch
            {
                < 0 => LineDomain.Bottom,
                0 => LineDomain.Line,
                > 0 => LineDomain.Top,
                _ => throw new ArithmeticException()
            };

        internal static RayDomain GetDomain(float2 ray, float2 testPoint) =>
            Determinant(ray, testPoint) switch
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
            float difference = x - y;
            float cyclicDifference = difference - math.floor(difference / 360) * 360;
            return cyclicDifference switch
            {
                0 => 0,
                < 180 => 1,
                180 => 0,
                > 180 => -1,
                _ => throw new ArithmeticException(),
            };
        }

        internal static float? GetDistanceFromOrigin(Line line, float2 ray)
        {
            float2 lineDiff = line.Left - line.Right;
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