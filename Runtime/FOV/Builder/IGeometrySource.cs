using Backstreets.Data;
using Backstreets.FOV.Geometry;

namespace Backstreets.FOV
{
    public interface IGeometrySource
    {
        PocketGeometry GetGeometry(PocketID pocket);
    }
}