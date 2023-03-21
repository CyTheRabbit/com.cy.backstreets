using UnityEngine;

namespace Backstreets
{
    public readonly struct PortalWindow
    {
        public PortalWindow(Vector2 origin, Vector2 leftBorder, Vector2 rightBorder)
        {
            Origin = origin;
            LeftBorder = leftBorder;
            RightBorder = rightBorder;
        }

        public Vector2 Origin { get; }
        public Vector2 LeftBorder { get; }
        public Vector2 RightBorder { get; }
    }
}