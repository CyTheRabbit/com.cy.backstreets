using Unity.Mathematics;

namespace Backstreets.FOV.Geometry
{
    internal readonly struct FieldOfViewSpace
    {
        private readonly float2 origin;

        public FieldOfViewSpace(float2 origin)
        {
            this.origin = origin;
        }

        public Line WorldToViewport(Line line) =>
            new(right: WorldToViewport(line.Right), left: WorldToViewport(line.Left));

        public float2 WorldToViewport(float2 point) => point - origin;

        public float2 ViewportToWorld(float2 point) => point + origin;
    }
}
