using Backstreets.FOV.Builder;
using Backstreets.FOV.Geometry;
using Unity.Collections;

namespace Backstreets.FOV.MeshBuilder
{
    internal struct MeshBuildingContext
    {
        public BuildRequest Request;
        public NativeArray<BoundSector> Sectors;

        public MeshBuildingContext(BuildRequest request, NativeArray<BoundSector> sectors)
        {
            Request = request;
            Sectors = sectors;
        }

        public FieldOfViewSpace Space => Request.FieldOfView.Space;
        public int VertexCount => Sectors.Length * 4;
        public int IndexCount => Sectors.Length * 6;
    }
}