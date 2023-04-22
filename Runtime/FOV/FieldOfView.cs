using Backstreets.FOV.Geometry;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV
{
    public struct FieldOfView : INativeDisposable
    {
        internal FieldOfViewSpace Space;
        internal NativeQueue<BoundSector> Sectors;
        internal NativeList<Line> ConflictingBounds;

        internal FieldOfView(FieldOfViewSpace space, Allocator allocator)
        {
            Sectors = new NativeQueue<BoundSector>(allocator);
            ConflictingBounds = new NativeList<Line>(ConflictingBoundsCapacity, allocator);
            Space = space;
        }

        public bool IsCreated => Sectors.IsCreated && ConflictingBounds.IsCreated;

        public int BoundsLength => Sectors.Count;

        public NativeArray<BoundSector> GetAllBoundSectors(Allocator allocator) =>
            Sectors.ToArray(allocator);

        public void Dispose()
        {
            if (!IsCreated) return;
            Sectors.Dispose();
            ConflictingBounds.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            if (!IsCreated) return default;
            return JobHandle.CombineDependencies(
                Sectors.Dispose(inputDeps),
                ConflictingBounds.Dispose(inputDeps));
        }

        private const int ConflictingBoundsCapacity = 32;
    }
}
