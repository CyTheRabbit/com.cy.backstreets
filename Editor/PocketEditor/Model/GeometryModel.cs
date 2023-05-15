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

        public GeometryModel(PocketPrefabDetails pocket, Action updateViewAction)
        {
            this.pocket = pocket;
            this.updateViewAction = updateViewAction;
            Edges = new EdgesAccess(pocket, model: this);
            Portals = new PortalsAccess(pocket, model: this);
        }

        public void UpdateView() => updateViewAction();

        internal RecordChangesScope RecordChanges(string name) => new(model: this, target: pocket, name);
    }
}
