using Backstreets.Data;

namespace Backstreets.FOV.Geometry
{
    public readonly struct Portal
    {
        public readonly Bound Bound;
        public readonly PocketID Entrance;
        public readonly PocketID Exit;
        public Line Edge => Bound.Line;
        public int EdgeIndex => Bound.EdgeIndex;

        public Portal(Line edge, int entranceEdgeIndex, PocketID entrance, PocketID exit)
        {
            Bound = new Bound(edge, entranceEdgeIndex);
            Entrance = entrance;
            Exit = exit;
        }
    }
}