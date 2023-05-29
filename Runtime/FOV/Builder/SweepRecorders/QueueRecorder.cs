using Backstreets.FOV.Geometry;
using Unity.Collections;

namespace Backstreets.FOV.Builder.SweepRecorders
{
    internal struct QueueRecorder : ISweepRecorder
    {
        [WriteOnly] private NativeQueue<Bound> bounds;
        private Bound currentBound;

        public QueueRecorder(NativeQueue<Bound> bounds)
        {
            this.bounds = bounds;
            currentBound = new Bound(line: default, InvalidEdgeIndex);
        }

        public void Start(in LineOfSight lineOfSight)
        {
            RecordBoundStart(in lineOfSight);
        }

        public void PreUpdate(in LineOfSight lineOfSight)
        {
            RecordBoundEnd(in lineOfSight);
        }

        public void Record(in LineOfSight lineOfSight, LineOfSight.UpdateReport update, Corner corner)
        {
            if (update.OperationFailed)
            {
                // conflictingBounds.AddNoResize(corner.Edge);
            }
            else if (update.ClosestEdgeChanged)
            {
                FlushBound();
                RecordBoundStart(in lineOfSight);
            }
        }

        public void End(in LineOfSight lineOfSight)
        {
            RecordBoundEnd(lineOfSight);
            FlushBound();
        }


        private void RecordBoundStart(in LineOfSight lineOfSight)
        {
            currentBound = new Bound
            {
                Right = lineOfSight.Raycast(),
                Left = default,
                EdgeIndex = lineOfSight.RaycastId(),
            };
        }

        private void RecordBoundEnd(in LineOfSight lineOfSight)
        {
            currentBound.Left = lineOfSight.Raycast();
        }

        private void FlushBound()
        {
            if (currentBound.EdgeIndex == InvalidEdgeIndex) return;

            bounds.Enqueue(currentBound);
        }


        private const int InvalidEdgeIndex = -1;
    }
}
