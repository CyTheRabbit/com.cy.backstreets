using Backstreets.FOV.Geometry;
using Unity.Collections;

namespace Backstreets.FOV.MeshBuilder
{
    internal struct MeshBuildingContext
    {
        public BuildRequest Request;
        public OrderedBoundSectors OrderedSectors;

        public MeshBuildingContext(BuildRequest request, OrderedBoundSectors sectors)
        {
            Request = request;
            OrderedSectors = sectors;
        }


        public NativeArray<BoundSector> Sectors => OrderedSectors.Sectors;
        public FieldOfViewSpace Space => Request.FieldOfView.Space;
        public int VertexCount => Sectors.Length * 4;
        public int IndexCount => Sectors.Length * 6;
    }
}
