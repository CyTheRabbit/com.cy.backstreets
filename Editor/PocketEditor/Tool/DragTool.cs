using System;
using System.Linq;
using Backstreets.Data;
using Backstreets.Editor.PocketEditor.Model;
using Backstreets.Editor.PocketEditor.View;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.Tool
{
    internal class DragTool : IGeometryTool
    {
        private readonly GeometryModel model;
        private CornerData[] dragCorners;
        private int controlID;

        public DragTool(GeometryModel model)
        {
            this.model = model;
            dragCorners = Array.Empty<CornerData>();
        }


        public GeometryType DrawMask => GeometryType.Everything;

        public GeometryType PickMask => GeometryType.Corner;


        public void OnBeforeView(Event @event)
        {
            bool isDraggingCorners = GUIUtility.hotControl == controlID;
            if (isDraggingCorners) ProcessDraggingEvent(@event);

            if (@event is { type: EventType.Layout })
            {
                controlID = GUIUtility.GetControlID(hint: GetHashCode(), FocusType.Passive);
            }
        }

        public void OnViewEvent(Event @event, GeometryID hotGeometry)
        {
            switch (@event)
            {
                case { type: EventType.MouseDown, button: 0 }
                    when hotGeometry is { Type: GeometryType.Corner }:
                {
                    float2 position = model.Corners.Get(hotGeometry).Position;
                    CaptureCornersAtPosition(position);
                    break;
                }
            }
        }

        public void Dispose()
        {
        }

        public void OnInspectorGUI()
        {
            GUILayout.Label("Drag corners to move");
        }

        public void CaptureCorners(params CornerData[] corners)
        {
            dragCorners = corners;
            GUIUtility.hotControl = controlID;
        }


        private void ProcessDraggingEvent(Event @event)
        {
            switch (@event)
            {
                case { type: EventType.MouseDrag, button: 0 }:
                {
                    Vector2 newPosition = ProjectOntoGeometry(@event.mousePosition);
                    MoveCorners(newPosition);
                    @event.Use();
                    break;
                }
                case { type: EventType.MouseUp, button: 0 }:
                {
                    dragCorners = Array.Empty<CornerData>();
                    break;
                }
                case { type: EventType.Repaint }:
                {
                    HighlightDragGeometry();
                    break;
                }
            }
        }

        private void MoveCorners(Vector2 position)
        {
            for (var i = 0; i < dragCorners.Length; i++)
            {
                dragCorners[i].Position = position;
            }

            model.Corners.UpdateBatch(dragCorners);
        }

        private void CaptureCornersAtPosition(float2 position) =>
            CaptureCorners(model.Corners.All
                .Where(corner => corner.Position.Equals(position))
                .ToArray());

        private void HighlightDragGeometry()
        {
            foreach (CornerData corner in dragCorners)
            {
                GeometryID edgeID = new(GeometryType.Edge, corner.EdgeID);
                EdgeData edge = model.Edges.Get(edgeID);

                using var drawingScope = new Handles.DrawingScope(Color.cyan);
                Handles.DrawLine(
                    math.float3(edge.right, 0),
                    math.float3(edge.left, 0));
            }
        }


        private static Vector2 ProjectOntoGeometry(Vector2 guiPoint)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(guiPoint);
            bool isOnZPlane = ray.origin.z == 0;
            if (isOnZPlane) return ray.origin;

            float distanceToZPlane = ray.origin.z / ray.direction.z;
            return ray.GetPoint(distance: distanceToZPlane);
        }
    }
}
