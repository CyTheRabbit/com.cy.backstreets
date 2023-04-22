using Backstreets.FOV.Geometry;

namespace Backstreets.FOV.Jobs.SweepRecorders
{
    internal interface ISweepRecorder
    {
        void Start(in LineOfSight lineOfSight);
        void PreUpdate(in LineOfSight lineOfSight);
        void Record(in LineOfSight lineOfSight, LineOfSight.UpdateReport update, Corner corner);
        void End(in LineOfSight lineOfSight);
    }
}