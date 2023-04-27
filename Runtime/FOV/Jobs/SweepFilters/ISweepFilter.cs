using Backstreets.FOV.Geometry;

namespace Backstreets.FOV.Jobs.SweepFilters
{
    internal interface ISweepFilter
    {
        bool ShouldProcess(Line edge);
    }
}