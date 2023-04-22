using Backstreets.FOV.Geometry;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV.Builder
{
    public struct FarBounds : INativeDisposable
    {
        public NativeQueue<Bound> Bounds;

        public FarBounds(Allocator allocator) => Bounds = new NativeQueue<Bound>(allocator);

        public void Dispose() => Bounds.Dispose();
        public JobHandle Dispose(JobHandle inputDeps) => Bounds.Dispose(inputDeps);
    }
}