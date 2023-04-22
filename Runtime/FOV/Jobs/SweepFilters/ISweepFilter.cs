using Backstreets.FOV.Geometry;

namespace Backstreets.FOV.Jobs.SweepFilters
{
    internal interface ISweepFilter
    {
        float RightLimit { get; }
        float LeftLimit { get; }
        bool ShouldProcess(Line edge);
    }
}