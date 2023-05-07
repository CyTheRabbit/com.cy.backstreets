using Backstreets.Data;

namespace Backstreets.FOV.Geometry
{
    public struct BoundSector
    {
        public Bound Near;
        public Bound Far;
        public PocketID Pocket;

        public BoundSector(Bound near, Bound far, PocketID pocket)
        {
            Near = near;
            Far = far;
            Pocket = pocket;
        }
    }
}