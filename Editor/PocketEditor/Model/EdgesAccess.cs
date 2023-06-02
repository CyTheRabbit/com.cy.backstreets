using Backstreets.Data;
using Backstreets.FOV.Geometry;
using Backstreets.Pocket;

namespace Backstreets.Editor.PocketEditor.Model
{
    internal class EdgesAccess
    {
        private readonly PocketPrefabDetails pocket;

        public EdgesAccess(PocketPrefabDetails pocket)
        {
            this.pocket = pocket;
        }

        public Line this[EdgeID id]
        {
            get => Get(id);
            set => Update(id, value);
        }

        public Line Get(EdgeID id) => pocket.Polygon[id];

        public void Update(EdgeID id, Line data) => pocket.Polygon[id] = data;

        public bool Exists(EdgeID id) => pocket.Polygon.IsValidID(id);
    }
}
