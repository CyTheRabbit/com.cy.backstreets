using Backstreets.FOV.Geometry;
using Unity.Mathematics;

namespace Backstreets.FOV.Builder.SweepFilters
{
    public readonly struct PortalFilter : ISweepFilter
    {
        private readonly Line portalEdge;

        public PortalFilter(Line portalEdge) => this.portalEdge = portalEdge;

        public bool ShouldProcess(Line edge) => IsEdgeAfterPortal(edge) && !IsPortalEdge(edge);

        private bool IsPortalEdge(Line edge) =>
            AreEqual(edge, portalEdge) ||
            AreEqual(edge.Reverse(), portalEdge);

        private static bool AreEqual(Line x, Line y) =>
            math.lengthsq(x.Right - y.Right) <= math.EPSILON &&
            math.lengthsq(x.Left - y.Left) <= math.EPSILON;

        private bool IsEdgeAfterPortal(Line edge) =>
            LineMath.CompareLineDistance(portalEdge, edge) < 0;
    }
}