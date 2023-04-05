using Backstreets.FOV.Geometry;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV
{
    public struct FieldOfView : INativeDisposable
    {
        internal FieldOfViewSpace Space;
        internal NativeList<Line> Bounds;
        internal NativeList<Line> ConflictingBounds;

        internal FieldOfView(FieldOfViewSpace space, int boundsCapacity, Allocator allocator)
        {
            Space = space;
            Bounds = new NativeList<Line>(boundsCapacity, allocator);
            ConflictingBounds = new NativeList<Line>(allocator);
        }

        public bool IsCreated => Bounds.IsCreated && ConflictingBounds.IsCreated;

        public void Dispose()
        {
            if (!IsCreated) return;
            Bounds.Dispose();
            ConflictingBounds.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            if (!IsCreated) return default;
            return JobHandle.CombineDependencies(
                Bounds.Dispose(inputDeps),
                ConflictingBounds.Dispose(inputDeps));
        }
    }
}
