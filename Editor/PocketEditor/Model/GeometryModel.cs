using System;
using Backstreets.Data;
using Backstreets.Pocket;

namespace Backstreets.Editor.PocketEditor.Model
{
    internal class GeometryModel
    {
        public event Action EdgeIDsUpdated;

        private readonly PocketPrefabDetails pocket;
        private readonly Action updateViewAction;

        public readonly EdgesAccess Edges;
        public readonly PortalsAccess Portals;
        public readonly CornersAccess Corners;

        public GeometryModel(PocketPrefabDetails pocket, Action updateViewAction)
        {
            this.pocket = pocket;
            this.updateViewAction = updateViewAction;
            Edges = new EdgesAccess(pocket);
            Portals = new PortalsAccess(pocket);
            Corners = new CornersAccess(pocket, model: this);
        }

        public void UpdateView() => updateViewAction();

        /// <summary>
        /// Recalculates IDs for edge references.
        /// Call whenever you change polygon structure.
        /// </summary>
        public void RemapEdgeIDs(Func<EdgeID, EdgeID?> remap)
        {
            for (int i = 0; i < pocket.Portals.Count; i++)
            {
                PortalData portal = pocket.Portals[i];
                if (remap(portal.edgeID) is not { } newID)
                {
                    pocket.Portals.RemoveAt(i--);
                    continue;
                }

                portal.edgeID = newID;
                pocket.Portals[i] = portal;
            }

            EdgeIDsUpdated?.Invoke();
        }

        internal RecordChangesScope RecordChanges(string name) => new(model: this, target: pocket, name);
    }
}
