using Backstreets.FOV.Geometry;

namespace Backstreets.FOV.Builder.SweepFilters
{
    internal interface ISweepFilter
    {
        bool ShouldProcess(Line edge);
    }
}