using System;
using Backstreets.Editor.PocketEditor.View;
using Backstreets.Pocket;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.Tool
{
    public class SelectionTool : IGeometryTool
    {
        private readonly PocketPrefabDetails pocket;

        public SelectionTool(PocketPrefabDetails pocket)
        {
            this.pocket = pocket;
        }

        public GeometryType DrawMask => GeometryType.Everything;

        public GeometryType PickMask => GeometryType.Portal;

        public void OnViewEvent(Event @event, GeometryID hotGeometry)
        {
            bool isLeftMouseClick = @event is { type: EventType.MouseUp, button: 0 };
            if (hotGeometry is { Type: GeometryType.Portal, ID: var portalID } && isLeftMouseClick)
            {
                int portalIndex = Array.FindIndex(pocket.Portals, portal => portal.edgeID == portalID);
                PortalSelection.Focus(pocket, portalIndex);
            }
        }

        public void OnInspectorGUI()
        {
            GUILayout.Label("Selection Tool");
        }

        public void Dispose()
        {
        }
    }
}
