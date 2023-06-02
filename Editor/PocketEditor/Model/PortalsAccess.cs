using System;
using System.Collections.Generic;
using Backstreets.Data;
using Backstreets.Editor.PocketEditor.View;
using Backstreets.Pocket;

namespace Backstreets.Editor.PocketEditor.Model
{
    internal class PortalsAccess
    {
        private readonly PocketPrefabDetails pocket;


        public PortalsAccess(PocketPrefabDetails pocket)
        {
            this.pocket = pocket;
        }


        protected List<PortalData> Portals => pocket.Portals;

        public PortalData this[GeometryID id]
        {
            get => Get(id);
            set => Update(id, value);
        }


        public PortalData Get(GeometryID id) => Portals.Find(Match(id));

        public void Update(GeometryID id, PortalData data)
        {
            int portalIndex = Portals.FindIndex(Match(id));
            Validation.AssertValidIndex(portalIndex, id);

            Portals[portalIndex] = data;
        }

        public void Delete(GeometryID id)
        {
            int portalIndex = Portals.FindIndex(Match(id));
            Validation.AssertValidIndex(portalIndex, id);

            Portals.RemoveAt(portalIndex);
        }

        public GeometryID Create(PortalData data)
        {
            GeometryID id = data;
            bool idAlreadyUsed = Portals.Exists(Match(data.edgeID));
            if (idAlreadyUsed) throw Validation.IDAlreadyUsed(id);

            Portals.Add(data);
            return id;
        }

        public bool Exists(GeometryID id) => Portals.Exists(Match(id));


        private static Predicate<PortalData> Match(EdgeID edgeID) =>
            data => data.edgeID == edgeID;
    }
}
