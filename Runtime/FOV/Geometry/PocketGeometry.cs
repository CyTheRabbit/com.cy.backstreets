using Backstreets.Data;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV.Geometry
{
    public struct PocketGeometry : INativeDisposable
    {
        public PocketID ID;
        public NativeArray<Line> Edges;
        public NativeArray<Portal> Portals;

        public PocketGeometry(PocketID id, int edgeCount, int portalCount, Allocator allocator)
        {
            ID = id;

            if (allocator == Allocator.None)
            {
                Edges = default;
                Portals = default;
            }
            else
            {
                Edges = new NativeArray<Line>(edgeCount, allocator);
                Portals = new NativeArray<Portal>(portalCount, allocator);
            }
        }

        public bool IsValid => Edges.IsCreated && Portals.IsCreated;

        public void Dispose()
        {
            Edges.Dispose();
            Portals.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps) => JobHandle.CombineDependencies(
            Edges.Dispose(inputDeps),
            Portals.Dispose(inputDeps));


        public static PocketGeometry Nothing(PocketID id) => new(id, 0, 0, Allocator.None);
    }
}