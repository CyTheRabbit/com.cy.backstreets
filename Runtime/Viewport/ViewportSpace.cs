using UnityEngine;

namespace Backstreets.Viewport
{
    internal readonly struct ViewportSpace
    {
        public readonly Vector2 Origin;
        public readonly Vector2 Normal;
        public readonly ViewportLine PortalLine;

        public ViewportSpace(PortalWindow window)
        {
            Origin = window.Origin;
            Normal = Vector2.Perpendicular(window.RightBorder - window.LeftBorder).normalized;
            PortalLine = new ViewportLine
            {
                Left = Convert(window.LeftBorder, Origin, Normal),
                Right = Convert(window.RightBorder, Origin, Normal)
            };
        }

        public ViewportPoint Convert(Vector2 worldPoint) => Convert(worldPoint, Origin, Normal);

        public Vector2 Convert(ViewportPoint localPoint) => Convert(localPoint, Origin);

        public ViewportLine MakeLine(Vector2 left, Vector2 right) => new()
        {
            Left = Convert(left, Origin, Normal),
            Right = Convert(right, Origin, Normal)
        };

        public ViewportLine? Clamp(ViewportLine line)
        {
            if (line.Left.Angle >= PortalLine.Right.Angle) return null;
            if (line.Right.Angle <= PortalLine.Left.Angle) return null;
            return new ViewportLine
            {
                Left = line.Left.Angle > PortalLine.Left.Angle ? line.Left : PortalLine.Left,
                Right = line.Right.Angle < PortalLine.Right.Angle ? line.Right : PortalLine.Right
            };
        }

        private static ViewportPoint Convert(Vector2 worldPoint, Vector2 origin, Vector2 normal)
        {
            Vector2 localPoint = worldPoint - origin;
            return new ViewportPoint
            {
                XY = localPoint,
                Angle = -Vector2.SignedAngle(normal, localPoint)
            };
        }

        private static Vector2 Convert(ViewportPoint localPoint, Vector2 origin) =>
            localPoint.XY + origin;
    }
}