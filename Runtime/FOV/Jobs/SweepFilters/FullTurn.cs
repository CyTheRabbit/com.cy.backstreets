using Backstreets.FOV.Geometry;

namespace Backstreets.FOV.Jobs.SweepFilters
{
    public readonly struct FullTurn : ISweepFilter
    {
        public bool ShouldProcess(Line edge) => true;
    }
}