using Backstreets.Data;
using Backstreets.FOV.Geometry;
using Unity.Collections;

namespace Backstreets.FOV.Utility
{
    public static class FieldOfViewFilter
    {
        public static NativeArray<BoundSector> GetBoundSectorsByPocket(this FieldOfView fov, PocketID pocket, Allocator allocator)
        {
            NativeArray<BoundSector> input = fov.GetAllBoundSectors(Allocator.Temp);
            NativeArray<BoundSector> filteredSectors = new(fov.BoundsLength, Allocator.Temp);

            int filteredCount = 0;
            foreach (BoundSector sector in input)
            {
                if (sector.Pocket == pocket)
                {
                    filteredSectors[filteredCount++] = sector;
                }
            }

            NativeArray<BoundSector> output = new NativeArray<BoundSector>(filteredCount, allocator);
            output.CopyFrom(filteredSectors.GetSubArray(0, filteredCount));

            {
                input.Dispose();
                filteredSectors.Dispose();
            }

            return output;
        }
    }
}
