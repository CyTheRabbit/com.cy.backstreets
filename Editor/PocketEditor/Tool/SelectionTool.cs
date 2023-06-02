using System;
using Backstreets.Data;
using Backstreets.Editor.PocketEditor.Model;
using Backstreets.Editor.PocketEditor.View;
using Backstreets.FOV.Geometry;
using Unity.Mathematics;
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
            float2 corner = model.Corners.Get(inspectedGeometry);

            using EditorGUI.ChangeCheckScope check = new();
            corner = EditorGUILayout.Vector2Field("Position", corner);

            if (check.changed)
            {
                using RecordChangesScope changes = model.RecordChanges("Update corner");
                model.Corners.Update(inspectedGeometry, corner);
            }
        }

        public void Dispose()
        {
        }

        private void DrawPortalInspector()
        {
            PortalData portalData = model.Portals[inspectedGeometry];
            using EditorGUI.ChangeCheckScope check = new();
            GUILayout.Label($"Portal {portalData.edgeID}");
            portalData.exitID = EditorGUILayout.IntField("Exit ID", portalData.exitID);

            if (check.changed)
            {
                using RecordChangesScope changes = model.RecordChanges("Update portal");
                model.Portals.Update(inspectedGeometry, portalData);
            }
        }

        private void DrawEdgeInspector()
        {
            Line edge = model.Edges.Get(inspectedGeometry);

            using EditorGUI.ChangeCheckScope check = new();
            edge.Right = EditorGUILayout.Vector2Field("Right", edge.Right);
            edge.Left = EditorGUILayout.Vector2Field("Left", edge.Left);

            if (check.changed)
            {
                using RecordChangesScope changes = model.RecordChanges("Update edge");
                model.Edges.Update(inspectedGeometry, edge);
            }
        }
    }
}
