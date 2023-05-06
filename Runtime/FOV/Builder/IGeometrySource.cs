using Backstreets.Data;
using Backstreets.FOV.Geometry;

namespace Backstreets.FOV.Builder
{
    public interface IGeometrySource
    {
        PocketGeometry GetGeometry(PocketID pocket);
    }
}