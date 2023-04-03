using Backstreets.FOV.Geometry;
using Unity.Collections;

namespace Backstreets.FOV.Jobs.SweepVisitors
{
    [BurstCompatible]
    internal struct FieldOfViewBuilderVisitor : ILineOfSightVisitor
    {
        [WriteOnly] private NativeList<Line> bounds;
        private Line currentBound;

        public FieldOfViewBuilderVisitor(NativeList<Line> bounds)
        {
            this.bounds = bounds;
            currentBound = default;
        }

        public bool ShouldProcess(Line line) => true;

        public void Start(in LineOfSight lineOfSight)
        {
            currentBound = new Line(right: lineOfSight.Raycast(), left: default);
        }

        public void PreUpdate(in LineOfSight lineOfSight)
        {
            currentBound.Left = lineOfSight.Raycast();
        }

        public void Update(in LineOfSight lineOfSight, LineOfSight.UpdateReport update)
        {
            if (update.ClosestObstacleChanged)
            {
                bounds.Add(currentBound);
                currentBound = new Line(right: lineOfSight.Raycast(), left: default);
            }
        }

        public void End(in LineOfSight lineOfSight)
        {
            currentBound.Left = lineOfSight.Raycast();
            bounds.Add(currentBound);
        }
    }
}