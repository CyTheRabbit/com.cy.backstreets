using Backstreets.FOV.Geometry;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV
{
    public struct FieldOfView : INativeDisposable
    {
        internal FieldOfViewSpace Space;
        internal NativeList<Line> Bounds;

        internal FieldOfView(FieldOfViewSpace space, int boundsCapacity, Allocator allocator)
        {
            Space = space;
            Bounds = new NativeList<Line>(boundsCapacity, allocator);
        }

        public void Dispose()
        {
            Bounds.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            return Bounds.Dispose(inputDeps);
        }
    }
}
