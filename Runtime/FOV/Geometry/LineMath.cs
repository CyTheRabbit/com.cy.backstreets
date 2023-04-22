using System;
using Unity.Mathematics;

namespace Backstreets.FOV.Geometry
{
    internal static class LineMath
    {
        private const float Epsilon = 0.0001f;

        private static float Determinant(float2 a, float2 b) => a.x * b.y - a.y * b.x;

        internal static float Angle(float2 point)
        {
            float distance = math.length(point);
            float sign = math.sign(point.y) switch { 0 when point.x < 0 => 1, var rawSign => rawSign };
            return distance < math.EPSILON ? 0 : math.degrees(math.acos(point.x / distance)) * sign;
        }

        internal static float2 Ray(float angle)
        {
            double radians = math.radians((double)angle);
            double2 rayDouble = new(math.cos(radians), math.sin(radians));
            return new float2(rayDouble);
        }

        internal static float2? GetIntersection(Line a, Line b)
        {
            float2 aDiff = a.Right - a.Left;
            float2 bDiff = b.Right - b.Left;
            float denominator = Determinant(aDiff, bDiff);
            if (denominator == 0) return null; // TODO: test if if-less is faster
            float aDeterminant = Determinant(a.Right, a.Left);
            float bDeterminant = Determinant(b.Right, b.Left);
            return (bDiff * aDeterminant - aDiff * bDeterminant) / denominator;
        }

        internal static float2? ProjectFromOrigin(Line line, float2 ray)
        {
            float2 lineDiff = line.Right - line.Left;
            float denominator = Determinant(lineDiff, ray);
            if (denominator == 0) return float2.zero;
            float distance = Determinant(line.Right, line.Left) / denominator;
            return ray * distance;
        }

        internal static Line? ClipLine(Line line, Line window)
        {
            bool facesOrigin = GetOriginDomain(line) is LineDomain.Bottom;
            if (!facesOrigin) return null;

            float lineRightAngle = Angle(line.Right);
            float lineLeftAngle = Angle(line.Left);
            float windowRightAngle = Angle(window.Right);
            float windowLeftAngle = Angle(window.Left);

            bool isOverlapping = CompareAngleCyclic(lineRightAngle, windowLeftAngle) < 0 &&
                                 CompareAngleCyclic(lineLeftAngle, windowRightAngle) > 0;
            if (!isOverlapping) return null;

            if (lineRightAngle < windowRightAngle && ProjectFromOrigin(line, window.Right) is { } newRight)
            {
                line.Right = newRight;
            }

            if (lineLeftAngle > windowLeftAngle && ProjectFromOrigin(line, window.Left) is { } newLeft)
            {
                line.Left = newLeft;
            }

            return line;
        }

        internal static LineDomain GetDomain(Line line, float2 testPoint) => 
            Determinant(line.Right - line.Left, line.Right - testPoint) switch
            {
                > Epsilon => LineDomain.Bottom,
                >= -Epsilon and <= Epsilon => LineDomain.Line,
                < -Epsilon => LineDomain.Top,
                _ => throw new ArithmeticException()
            };

        internal static LineDomain GetDomain(Line line, Line testLine) => Combine(
                GetDomain(line, testLine.Right),
                GetDomain(line, testLine.Left));

        internal static RayDomain GetDomain(float2 ray, float2 testPoint) =>
            Determinant(ray, testPoint) switch
            {
                < -Epsilon => RayDomain.Right,
                >= -Epsilon and <= Epsilon => RayDomain.Straight,
                > Epsilon => RayDomain.Left,
                _ => throw new ArithmeticException()
            };

        internal static LineDomain GetOriginDomain(Line line) =>
            // If you take GetDomain method and substitute testPoint with Vector2.zero,
            // in the end you get the following expression:
            Determinant(line.Right, line.Left) switch
            {
                > Epsilon => LineDomain.Bottom,
                >= -Epsilon and <= Epsilon => LineDomain.Line,
                < -Epsilon => LineDomain.Top,
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

        public static int CompareLineDistance(Line x, Line y)
        {
            const float cutoffRatio = 0.01f;

            // Shrink lines insignificantly in case they have a shared corner.
            Line xShrunk = Shrink(x);
            Line yShrunk = Shrink(y);

            return SolveFor(xShrunk, yShrunk) ?? -SolveFor(yShrunk, xShrunk) ?? 0;


            static int? SolveFor(Line main, Line other)
            {
                LineDomain rightDomain = GetDomain(main, other.Right);
                LineDomain leftDomain = GetDomain(main, other.Left);
                LineDomain otherDomain = Combine(rightDomain, leftDomain);
                LineDomain originDomain = GetOriginDomain(main);
                return otherDomain switch
                {
                    LineDomain.Both => null,
                    LineDomain.Line => 0,
                    _ => otherDomain == originDomain ? 1 : -1,
                };
            }

            static Line Shrink(Line line) => new(
                right: math.lerp(line.Right, line.Left, cutoffRatio),
                left: math.lerp(line.Left, line.Right, cutoffRatio));
        }

        internal static float? GetDistanceFromOrigin(Line line, float2 ray)
        {
            float2 lineDiff = line.Right - line.Left;
            float denominator = Determinant(ray, lineDiff);
            if (denominator == 0) return null;
            float determinant = Determinant(line.Right, line.Left);
            float distance = -determinant / denominator;
            return distance;
        }


        /// <summary>
        /// Since <see cref="Line"/> uses right-left notation, to define domains relative to the line
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
            Right,
            Left,
        }

        public static float NormalizeAngle(float angle)
        {
            float positiveAngle = angle + 180; // angle in range 0..360
            float positiveNormalized = positiveAngle - math.floor(positiveAngle / 360) * 360;
            return positiveNormalized - 180;
        }
    }
}