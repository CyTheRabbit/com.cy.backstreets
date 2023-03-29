using UnityEngine;

namespace Backstreets.FieldOfView
{
    internal readonly struct FieldOfViewSpace
    {
        private readonly Matrix4x4 worldToViewport;
        private readonly Matrix4x4 viewportToWorld;

        public FieldOfViewSpace(Matrix4x4 worldToViewport, Matrix4x4 viewportToWorld)
        {
            this.worldToViewport = worldToViewport;
            this.viewportToWorld = viewportToWorld;
        }

        public Corner MakeCorner(Vector2 vertex, Vector2 prev, Vector2 next)
        {
            Vector2 position = WorldToViewport(vertex);
            return new Corner
            {
                Position = position,
                Left = WorldToViewport(prev),
                Right = WorldToViewport(next),
                Angle = Angle(position),
            };
        }

        public Vector2 WorldToViewport(Vector2 point) => Convert(point, in worldToViewport);

        public Vector2 ViewportToWorld(Vector2 point) => Convert(point, in viewportToWorld);


        private static Vector2 Convert(Vector2 point, in Matrix4x4 matrix) => new()
        {
            x = (float)((double)matrix.m00 * point.x + (double)matrix.m01 * point.y + matrix.m03),
            y = (float)((double)matrix.m10 * point.x + (double)matrix.m11 * point.y + matrix.m13)
        };

        private static float Angle(Vector2 viewportPoint) => 
            Vector2.Angle(Vector2.right, viewportPoint) * Mathf.Sign(viewportPoint.y);
    }
}