using Backstreets.FOV.Geometry;
using Unity.Collections;

namespace Backstreets.FOV.Jobs.SweepRecorders
{
    [BurstCompatible]
    internal struct FieldOfViewRecorder : ISweepRecorder
    {
        [WriteOnly] private FieldOfView fieldOfView;
        private Line currentBound;
        private int currentEdgeIndex;

        public FieldOfViewRecorder(FieldOfView fieldOfView)
        {
            this.fieldOfView = fieldOfView;
            currentBound = default;
            currentEdgeIndex = InvalidEdgeIndex;
        }

        public float RightLimit => -180;
        public float LeftLimit => 180;

        public bool ShouldProcess(Line line) => true;

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
                fieldOfView.ConflictingBounds.Add(corner.Edge);
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
            currentBound = new Line(right: lineOfSight.Raycast(), left: default);
            currentEdgeIndex = lineOfSight.RaycastId();
        }

        private void RecordBoundEnd(in LineOfSight lineOfSight)
        {
            currentBound.Left = lineOfSight.Raycast();
        }

        private void FlushBound()
        {
            fieldOfView.Add(currentBound, currentEdgeIndex);
        }


        private const int InvalidEdgeIndex = -1;
    }
}