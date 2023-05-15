using System;
using Backstreets.Data;
using Backstreets.Editor.PocketEditor.View;
using Backstreets.Pocket;

namespace Backstreets.Editor.PocketEditor.Model
{
    internal class PortalsAccess : DataAccess<PortalData>
    {
        public PortalsAccess(PocketPrefabDetails pocket, GeometryModel model) : base(pocket, model)
        {
        }

        protected override GeometryType SupportedType => GeometryType.Portal;

        protected override PortalData[] GetDataCollection() => Pocket.Portals;

        protected override void SetDataCollection(PortalData[] collection) => Pocket.Portals = collection;

        protected override GeometryID GetID(PortalData data) => GeometryID.Of(data);

        protected override void AssignID(GeometryID id, ref PortalData data) => data.edgeID = id.ID;

        protected override GeometryID GetNewID() => throw new NotSupportedException();
    }
}
