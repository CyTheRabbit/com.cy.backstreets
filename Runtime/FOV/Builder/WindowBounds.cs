using Backstreets.FOV.Geometry;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV.Builder
{
    public struct WindowBounds : INativeDisposable
    {
        public NativeQueue<BoundSector> Sectors;

        public WindowBounds(Allocator allocator) => Sectors = new NativeQueue<BoundSector>(allocator);

        public void Dispose() => Sectors.Dispose();
        public JobHandle Dispose(JobHandle inputDeps) => Sectors.Dispose(inputDeps);
    }
}
