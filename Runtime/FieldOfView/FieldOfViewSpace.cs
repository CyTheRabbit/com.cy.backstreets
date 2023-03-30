using Unity.Mathematics;

namespace Backstreets.FieldOfView
{
    internal readonly struct FieldOfViewSpace
    {
        private readonly float2 origin;

        public FieldOfViewSpace(float2 origin)
        {
            this.origin = origin;
        }

        public Corner MakeCorner(float2 vertex, float2 prev, float2 next)
        {
            float2 position = WorldToViewport(vertex);
            return new Corner
            {
                Position = position,
                Left = WorldToViewport(prev),
                Right = WorldToViewport(next),
                Angle = LineMath.Angle(position),
            };
        }

        public float2 WorldToViewport(float2 point) => point - origin;

        public float2 ViewportToWorld(float2 point) => point + origin;
    }
}