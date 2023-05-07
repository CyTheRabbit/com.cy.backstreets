using Backstreets.FOV.Geometry;
using Unity.Collections;

namespace Backstreets.FOV.Builder.SweepRecorders
{
    public struct QueuePortalSectorRecorder : ISweepSectorRecorder
    {
        [WriteOnly] private NativeQueue<BoundSector> sectors;
        private readonly int portalEdgeIndex;

        public QueuePortalSectorRecorder(NativeQueue<BoundSector> sectors, int portalEdgeIndex)
        {
            this.sectors = sectors;
            this.portalEdgeIndex = portalEdgeIndex;
        }

        public void Record(BoundSector sector)
        {
            if (sector.Near.EdgeIndex != portalEdgeIndex) return;

            sectors.Enqueue(sector);
        }
    }
}