using UnityEngine;

namespace Backstreets.Viewport
{
    public static class ViewportMath
    {
        internal static bool AnglesIntersect(ViewportLine a, ViewportLine b)
        {
            // Assuming both face origin
            return a.Left.Angle < b.Right.Angle && a.Right.Angle > b.Left.Angle;
        }

        internal static Vector2? GetIntersection(ViewportLine a, ViewportLine b)
        {
            float x1 = a.Left.XY.x, x2 = a.Right.XY.x, x3 = b.Left.XY.x, x4 = b.Right.XY.x;
            float y1 = a.Left.XY.y, y2 = a.Right.XY.y, y3 = b.Left.XY.y, y4 = b.Right.XY.y;
            float denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (denominator == 0) return null; // TODO: test if if-less is faster
            float aDeterminant = x1 * y2 - y1 * x2;
            float bDeterminant = x3 * y4 - x4 * y3;
            return new Vector2(
                x: ((x3 - x4) * aDeterminant - (x1 - x2) * bDeterminant) / denominator,
                y: ((y3 - y4) * aDeterminant - (y1 - y2) * bDeterminant) / denominator);
        }

        internal static Vector2? ProjectFromOrigin(ViewportLine line, Vector2 rayFromOrigin)
        {
            float x1 = line.Left.XY.x, x2 = line.Right.XY.x, x3 = rayFromOrigin.x;
            float y1 = line.Left.XY.y, y2 = line.Right.XY.y, y3 = rayFromOrigin.y;
            float denominator = x3 * (y1 - y2) - y3 * (x1 - x2);
            if (denominator == 0) return null;
            float determinant = x1 * y2 - y1 * x2;
            float distance = - determinant / denominator;
            return rayFromOrigin * distance;
        }

        internal static ViewportPoint? ProjectFromOrigin(ViewportLine line, ViewportPoint rayFromOrigin)
        {
            if (ProjectFromOrigin(line, rayFromOrigin.XY) is not { } projection) return null;

            bool projectedOnOppositeSide = Vector2.Dot(rayFromOrigin.XY, projection) > 0;
            float angle = projectedOnOppositeSide
                ? Mathf.Repeat(rayFromOrigin.Angle, 360) - 180
                : rayFromOrigin.Angle;
            return new ViewportPoint(projection, rayFromOrigin.Angle);
        }
    }
}