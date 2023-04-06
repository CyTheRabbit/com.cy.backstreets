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
        public NativeArray<Line> Lines;

        public PocketGeometry(int length) => Lines = new NativeArray<Line>(length, Allocator.TempJob);

        public void Dispose() => Lines.Dispose();

        public JobHandle Dispose(JobHandle inputDeps) => Lines.Dispose(inputDeps);
    }
}