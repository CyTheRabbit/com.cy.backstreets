using System.Linq;
using Backstreets.Data;
using Backstreets.Editor.PocketEditor.View;
using Backstreets.Pocket;

namespace Backstreets.Editor.PocketEditor.Model
{
    internal class EdgesAccess : DataAccess<EdgeData>
    {
        public EdgesAccess(PocketPrefabDetails pocket, GeometryModel model) : base(pocket, model)
        {
        }

        protected override GeometryType SupportedType => GeometryType.Edge;

        protected override EdgeData[] GetDataCollection() => Pocket.Edges;

        protected override void SetDataCollection(EdgeData[] collection) => Pocket.Edges = collection;

        protected override GeometryID GetID(EdgeData data) => GeometryID.Of(data);

        protected override void AssignID(GeometryID id, ref EdgeData data) => data.id = id.ID;

        protected override GeometryID GetNewID()
        {
            int maxID = Pocket.Edges.Max(edge => edge.id);
            int newID = maxID + 1;
            return new GeometryID(GeometryType.Edge, newID);
        }
    }
}
