using System;
using Backstreets.Data;
using Backstreets.Editor.PocketEditor.View;
using Backstreets.Pocket;
using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.Tool
{
    public class SelectionTool : IGeometryTool
    {
        private readonly PocketPrefabDetails pocket;
        private GeometryID inspectedGeometry;

        public SelectionTool(PocketPrefabDetails pocket)
        {
            this.pocket = pocket;
            inspectedGeometry = GeometryID.None;
        }

        public GeometryType DrawMask => GeometryType.Everything;

        public GeometryType PickMask => GeometryType.Portal;

        public void OnViewEvent(Event @event, GeometryID hotGeometry)
        {
            bool isLeftMouseClick = @event is { type: EventType.MouseUp, button: 0 };
            if (isLeftMouseClick)
            {
                inspectedGeometry = hotGeometry;
                EditorUtility.SetDirty(pocket); // hack to immediately repaint inspector
            }
        }

        public void OnInspectorGUI()
        {
            switch (inspectedGeometry.Type)
            {
                case GeometryType.None:
                    GUILayout.Label("Click geometry to start inspection");
                    break;
                case GeometryType.Portal:
                    DrawPortalInspector();
                    break;
                default:
                    GUILayout.Label($"Cannot inspect {inspectedGeometry.Type}");
                    break;
            }
        }

        public void Dispose()
        {
        }

        private void DrawPortalInspector()
        {
            int portalIndex = Array.FindIndex(pocket.Portals, portal => portal.edgeID == inspectedGeometry.ID);
            if (portalIndex < 0)
            {
                GUILayout.Label("Unknown Portal");
                return;
            }

            using EditorGUI.ChangeCheckScope check = new();

            PortalData portalData = pocket.Portals[portalIndex];
            GUILayout.Label($"Portal {portalData.edgeID}");
            portalData.edgeID = EditorGUILayout.IntField("Edge ID", portalData.edgeID);
            portalData.exitID = EditorGUILayout.IntField("Exit ID", portalData.exitID);

            if (check.changed)
            {
                pocket.Portals[portalIndex] = portalData;
                inspectedGeometry = GeometryID.Of(portalData);
                EditorUtility.SetDirty(pocket);
            }
        }
    }
}
