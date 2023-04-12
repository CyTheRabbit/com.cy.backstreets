using Backstreets.Data;
using Backstreets.FOV.Geometry;
using Backstreets.FOV.Jobs;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV
{
    public interface IGeometrySource
    {
        JobPromise<PocketGeometry> GetGeometry(PocketID pocket);
    }

    public struct PocketGeometry : INativeDisposable
    {
        public NativeArray<Line> Edges;

        public PocketGeometry(int length) => Edges = new NativeArray<Line>(length, Allocator.TempJob);

        public void Dispose() => Edges.Dispose();

        public JobHandle Dispose(JobHandle inputDeps) => Edges.Dispose(inputDeps);
    }
}