using Backstreets.FOV.Geometry;
using Unity.Collections;

namespace Backstreets.FOV.Jobs.SweepVisitors
{
    [BurstCompatible]
    internal struct FieldOfViewBuilderVisitor : ILineOfSightVisitor
    {
        [WriteOnly] private FieldOfView fieldOfView;
        private Line currentBound;

        public FieldOfViewBuilderVisitor(FieldOfView fieldOfView)
        {
            this.fieldOfView = fieldOfView;
            currentBound = default;
        }

        public float RightLimit => -180;
        public float LeftLimit => 180;
        public bool ShouldProcess(Line line) => true;

        public void Start(in LineOfSight lineOfSight)
        {
            currentBound = new Line(right: lineOfSight.Raycast(), left: default);
        }

        public void PreUpdate(in LineOfSight lineOfSight)
        {
            currentBound.Left = lineOfSight.Raycast();
        }

        public void Update(in LineOfSight lineOfSight, LineOfSight.UpdateReport update, Corner corner)
        {
            if (update.OperationFailed)
            {
                fieldOfView.ConflictingBounds.Add(corner.Line);
            }
            else if (update.ClosestObstacleChanged)
            {
                fieldOfView.Bounds.Add(currentBound);
                currentBound = new Line(right: lineOfSight.Raycast(), left: default);
            }
        }

        public void End(in LineOfSight lineOfSight)
        {
            currentBound.Left = lineOfSight.Raycast();
            fieldOfView.Bounds.Add(currentBound);
        }
    }
}