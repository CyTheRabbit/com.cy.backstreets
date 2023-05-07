using Backstreets.FOV.Geometry;
using Unity.Collections;

namespace Backstreets.FOV.Builder.SweepRecorders
{
    public struct QueueSectorRecorder : ISweepSectorRecorder
    {
        [WriteOnly] private NativeQueue<BoundSector> sectors;

        public QueueSectorRecorder(NativeQueue<BoundSector> sectors) => this.sectors = sectors;

        public void Record(BoundSector sector) => sectors.Enqueue(sector);
    }
}