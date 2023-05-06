using Backstreets.Data;
using Backstreets.FOV.Geometry;
using Backstreets.FOV.Utility;
using Unity.Collections;

namespace Backstreets.FOV.Builder
{
    internal class PotentiallyVisibleSetBuilder
    {
        private readonly IGeometrySource source;


        public PotentiallyVisibleSetBuilder(IGeometrySource source)
        {
            this.source = source;
        }


        public PotentiallyVisibleSet Build(Allocator allocator, PocketID originPocket, FieldOfViewSpace space)
        {
            PotentiallyVisibleSet set = new(originPocket, portalsCapacity: 16, pocketsCapacity: 8, allocator);

            PushOriginPortals(ref set, originPocket, space);

            foreach (PotentiallyVisibleSet.Window window in set.VisibleWindows.EnumerateResizeable())
            {
                PushPortals(window, ref set, space);
            }

            return set;
        }

        private void PushPortals(PotentiallyVisibleSet.Window window, ref PotentiallyVisibleSet set, FieldOfViewSpace space)
        {
            PocketGeometry geometry = source.GetGeometry(window.Exit);
            foreach (Portal portal in geometry.Portals)
            {
                set.TryAdd(portal: ConvertToViewport(portal, space), throughWindow: window);
            }
        }

        private Portal ConvertToViewport(Portal portal, FieldOfViewSpace space) => 
            new(edge: space.WorldToViewport(portal.Edge), portal.EdgeIndex, portal.Entrance, portal.Exit);

        private void PushOriginPortals(ref PotentiallyVisibleSet set, PocketID pocket, FieldOfViewSpace space)
        {
            PocketGeometry geometry = source.GetGeometry(pocket);
            foreach (Portal portal in geometry.Portals)
            {
                set.TryAdd(ConvertToViewport(portal, space));
            }
        }
    }
}