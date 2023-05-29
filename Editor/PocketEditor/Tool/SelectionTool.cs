using System;
using Backstreets.Data;
using Backstreets.Editor.PocketEditor.Model;
using Backstreets.Editor.PocketEditor.View;
using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.Tool
{
    internal class SelectionTool : IGeometryTool
    {
        private readonly GeometryModel model;
        private GeometryID inspectedGeometry;

        public SelectionTool(GeometryModel model)
        {
            this.model = model;
            inspectedGeometry = GeometryID.None;
        }

        public GeometryType DrawMask => GeometryType.Everything;

        public GeometryType PickMask => GeometryType.Edge | GeometryType.Portal | GeometryType.Corner;

        public void OnViewEvent(Event @event, GeometryID hotGeometry)
        {
            bool isLeftMouseClick = @event is { type: EventType.MouseUp, button: 0 };
            if (isLeftMouseClick)
            {
                inspectedGeometry = hotGeometry;
                model.UpdateView();
            }
        }

        public void OnInspectorGUI()
        {
            try
            {
                switch (inspectedGeometry.Type)
                {
                    case GeometryType.None:
                        GUILayout.Label("Click geometry to start inspection");
                        break;
                    case GeometryType.Edge:
                        DrawEdgeInspector();
                        break;
                    case GeometryType.Corner:
                        DrawCornerInspector();
                        break;
                    case GeometryType.Portal:
                        DrawPortalInspector();
                        break;
                    default:
                        GUILayout.Label($"Cannot inspect {inspectedGeometry.Type}");
                        break;
                }
            }
            catch (Exception e)
            {
                GUILayout.Label(e.Message);
            }
        }

        private void DrawCornerInspector()
        {
            CornerData cornerData = model.Corners.Get(inspectedGeometry);

            using EditorGUI.ChangeCheckScope check = new();
            cornerData.Position = EditorGUILayout.Vector2Field("Position", cornerData.Position);

            if (check.changed)
            {
                model.Corners.Update(cornerData);
            }
        }

        public void Dispose()
        {
        }

        private void DrawPortalInspector()
        {
            PortalData portalData = model.Portals.Get(inspectedGeometry);
            using EditorGUI.ChangeCheckScope check = new();
            GUILayout.Label($"Portal {portalData.edgeID}");
            portalData.edgeID = EditorGUILayout.IntField("Edge ID", portalData.edgeID);
            portalData.exitID = EditorGUILayout.IntField("Exit ID", portalData.exitID);

            if (check.changed)
            {
                inspectedGeometry = model.Portals.Update(inspectedGeometry, portalData);
            }
        }

        private void DrawEdgeInspector()
        {
            EdgeData edgeData = model.Edges.Get(inspectedGeometry);

            using EditorGUI.ChangeCheckScope check = new();
            edgeData.id = EditorGUILayout.IntField("Edge ID", edgeData.id);
            edgeData.right = EditorGUILayout.Vector2Field("Right", edgeData.right);
            edgeData.left = EditorGUILayout.Vector2Field("Left", edgeData.left);

            if (GUILayout.Button("Swap corners"))
            {
                (edgeData.right, edgeData.left) = (edgeData.left, edgeData.right);
            }

            if (check.changed)
            {
                inspectedGeometry = model.Edges.Update(inspectedGeometry, edgeData);
            }
        }
    }
}
