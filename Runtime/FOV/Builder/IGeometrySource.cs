using System;
using Backstreets.Data;
using Backstreets.FOV.Geometry;

namespace Backstreets.FOV.Builder
{
    public interface IGeometrySource : IDisposable
    {
        PocketGeometry GetGeometry(PocketID pocket);
    }
}