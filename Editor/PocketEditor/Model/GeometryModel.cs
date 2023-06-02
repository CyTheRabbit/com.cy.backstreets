using System;
using Backstreets.Pocket;

namespace Backstreets.Editor.PocketEditor.Model
{
    internal class GeometryModel
    {
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
            Corners = new CornersAccess(pocket);
        }

        public void UpdateView() => updateViewAction();

        internal RecordChangesScope RecordChanges(string name) => new(model: this, target: pocket, name);
    }
}
