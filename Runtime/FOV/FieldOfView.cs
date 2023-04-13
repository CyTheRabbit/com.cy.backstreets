using Backstreets.FOV.Geometry;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV
{
    public struct FieldOfView : INativeDisposable
    {
        internal FieldOfViewSpace Space;
        internal NativeList<Line> Bounds;
        internal NativeList<int> EdgeIndices;
        internal NativeList<Line> ConflictingBounds;
        internal int BoundsCapacity;

        internal FieldOfView(FieldOfViewSpace space, int boundsCapacity, Allocator allocator)
        {
            Space = space;
            BoundsCapacity = boundsCapacity;
            Bounds = new NativeList<Line>(boundsCapacity, allocator);
            EdgeIndices = new NativeList<int>(boundsCapacity, allocator);
            ConflictingBounds = new NativeList<Line>(allocator);
        }

        public bool IsCreated => Bounds.IsCreated && EdgeIndices.IsCreated && ConflictingBounds.IsCreated;

        internal void Add(Line bound, int edgeIndex)
        {
            Bounds.Add(bound);
            EdgeIndices.Add(edgeIndex);
        }

        public void Dispose()
        {
            if (!IsCreated) return;
            Bounds.Dispose();
            EdgeIndices.Dispose();
            ConflictingBounds.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            if (!IsCreated) return default;
            return JobHandle.CombineDependencies(
                Bounds.Dispose(inputDeps),
                EdgeIndices.Dispose(inputDeps),
                ConflictingBounds.Dispose(inputDeps));
        }
    }
}
