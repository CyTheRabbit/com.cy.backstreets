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

        public GeometryType PickMask => GeometryType.Edge | GeometryType.Portal;

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
                case GeometryType.Edge:
                    DrawEdgeInspector();
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

        private void DrawEdgeInspector()
        {
            int edgeIndex = Array.FindIndex(pocket.Edges, edge => edge.id == inspectedGeometry.ID);
            if (edgeIndex < 0)
            {
                GUILayout.Label("Unknown Edge");
                return;
            }

            using EditorGUI.ChangeCheckScope check = new();

            EdgeData edgeData = pocket.Edges[edgeIndex];
            edgeData.id = EditorGUILayout.IntField("Edge ID", edgeData.id);
            edgeData.right = EditorGUILayout.Vector2Field("Right", edgeData.right);
            edgeData.left = EditorGUILayout.Vector2Field("Left", edgeData.left);

            if (GUILayout.Button("Flip left and right"))
            {
                (edgeData.right, edgeData.left) = (edgeData.left, edgeData.right);
            }

            if (check.changed)
            {
                pocket.Edges[edgeIndex] = edgeData;
                inspectedGeometry = GeometryID.Of(edgeData);
                EditorUtility.SetDirty(pocket);
            }
        }
    }
}
