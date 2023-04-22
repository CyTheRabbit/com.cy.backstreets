using Backstreets.FOV.Geometry;

namespace Backstreets.FOV.Jobs.SweepFilters
{
    public readonly struct FullTurn : ISweepFilter
    {
        public float RightLimit => -180;
        public float LeftLimit => 180;
        public bool ShouldProcess(Line edge) => true;
    }
}