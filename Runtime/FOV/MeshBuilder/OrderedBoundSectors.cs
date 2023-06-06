using System.Collections.Generic;
using Backstreets.Data;
using Backstreets.FOV.Geometry;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV.MeshBuilder
{
    public struct OrderedBoundSectors : INativeDisposable
    {
        public NativeArray<BoundSector> Sectors;
        public NativeList<PocketID> PocketIDs;
        public NativeList<IndexRange> Ranges;


        public OrderedBoundSectors(ref FieldOfView fov, Allocator allocator)
        {
            if (fov.BoundsLength == 0)
            {
                Sectors = new NativeArray<BoundSector>(0, allocator);
                PocketIDs = new NativeList<PocketID>(0, allocator);
                Ranges = new NativeList<IndexRange>(0, allocator);
                return;
            }

            Sectors = fov.GetAllBoundSectors(allocator);
            Sectors.Sort(new CompareSectorsByPocket());

            PocketIDs = new NativeList<PocketID>(0, allocator);
            Ranges = new NativeList<IndexRange>(0, allocator);
            BuildRanges(Sectors, PocketIDs, Ranges);
        }


        public NativeSlice<BoundSector> this[PocketID pocket] =>
            FindRange(pocket) switch
            {
                null => default,
                var (start, length) => Sectors.Slice(start, length),
            };


        public void Dispose()
        {
            Sectors.Dispose();
            PocketIDs.Dispose();
            Ranges.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps) =>
            JobHandle.CombineDependencies(
                Sectors.Dispose(inputDeps),
                PocketIDs.Dispose(inputDeps),
                Ranges.Dispose(inputDeps));


        private int FindIndex(PocketID pocket)
        {
            for (int i = 0; i < PocketIDs.Length; i++)
            {
                if (PocketIDs[i] == pocket) return i;
            }

            return -1;
        }

        private IndexRange? FindRange(PocketID pocket) =>
            FindIndex(pocket) switch
            {
                -1 => null,
                var index => Ranges[index]
            };


        private static void BuildRanges(NativeArray<BoundSector> sectors, NativeList<PocketID> pockets, NativeList<IndexRange> ranges)
        {
            PocketID currentPocket;
            IndexRange currentRange;

            void StartRange(int index)
            {
                pockets.Add(currentPocket = sectors[index].Pocket);
                currentRange = new IndexRange { Start = index };
            }

            void EndRange(int firstIndexOutsideRange)
            {
                currentRange.Length = firstIndexOutsideRange - currentRange.Start;
                ranges.Add(currentRange);
            }

            StartRange(0);
            for (int i = 1; i < sectors.Length; i++)
            {
                if (currentPocket == sectors[i].Pocket) continue;

                EndRange(i);
                StartRange(i);
            }
            EndRange(sectors.Length);
        }


        private struct CompareSectorsByPocket : IComparer<BoundSector>
        {
            public int Compare(BoundSector x, BoundSector y) =>
                x.Pocket.ID.CompareTo(y.Pocket.ID);
        }
    }
}
