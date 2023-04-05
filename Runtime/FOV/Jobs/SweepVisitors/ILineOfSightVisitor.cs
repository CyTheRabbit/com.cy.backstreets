using Backstreets.FOV.Geometry;

namespace Backstreets.FOV.Jobs
{
    internal interface ILineOfSightVisitor
    {
        bool ShouldProcess(Line line);
        void Start(in LineOfSight lineOfSight);
        void PreUpdate(in LineOfSight lineOfSight);
        void Update(in LineOfSight lineOfSight, LineOfSight.UpdateReport update, Corner corner);
        void End(in LineOfSight lineOfSight);
    }
}