using System;
using System.Linq;
using Backstreets.Data;
using Backstreets.Editor.PocketEditor.Model;
using Backstreets.Editor.PocketEditor.View;
using Backstreets.FOV.Geometry;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.Tool
{
    internal class DeformTool : IGeometryTool
    {
        private readonly GeometryModel model;
        private CornerData[] dragCorners;

        public DeformTool(GeometryModel model)
        {
            this.model = model;
            dragCorners = Array.Empty<CornerData>();
        }


        public GeometryType DrawMask => GeometryType.Everything;

        public GeometryType PickMask => GeometryType.Edge | GeometryType.Corner;


        public void OnBeforeView(Event @event)
        {
            bool isDraggingCorners = dragCorners is { Length: > 0 };
            switch (@event)
            {
                case { type: EventType.MouseDrag, button: 0 }
                    when isDraggingCorners:
                {
                    Vector2 newPosition = ProjectOntoGeometry(@event.mousePosition);
                    MoveCorners(newPosition);
                    @event.Use();
                    break;
                }
                case { type: EventType.MouseUp, button: 0 }
                    when isDraggingCorners:
                {
                    dragCorners = Array.Empty<CornerData>();
                    break;
                }
                case { type: EventType.Repaint }
                    when isDraggingCorners:
                {
                    DrawSplitPreview();
                    break;
                }
            }
        }

        public void OnViewEvent(Event @event, GeometryID hotGeometry)
        {
            if (@event is { type: EventType.MouseDown, button: 0 })
            {
                switch (hotGeometry)
                {
                    case { Type: GeometryType.Edge }:
                        PerformSplit(@event, hotGeometry);
                        break;
                    case { Type: GeometryType.Corner }:
                        CaptureCorners(hotGeometry);
                        break;
                }
            }
        }

        public void Dispose()
        {
        }

        public void OnInspectorGUI()
        {
            GUILayout.Label("Click on edges to split");
            GUILayout.Label("Drag corners to move");
        }


        private void MoveCorners(Vector2 position)
        {
            for (var i = 0; i < dragCorners.Length; i++)
            {
                dragCorners[i].Position = position;
            }

            model.Corners.UpdateBatch(dragCorners);
        }

        private void DrawSplitPreview()
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

        private void PerformSplit(Event @event, GeometryID hotGeometry)
        {
            EdgeData edge = model.Edges.Get(hotGeometry);
            if (CalculateSplit(edge, @event) is not { } split) return;

            EdgeData rightCut = new() { id = edge.id, right = edge.right, left = split };
            EdgeData leftCut = new() { id = default, right = split, left = edge.left };

            model.Edges.Update(hotGeometry, rightCut);
            model.Edges.Create(ref leftCut);

            dragCorners = new CornerData[]
            {
                new(rightCut, CornerData.Endpoint.Left),
                new(leftCut, CornerData.Endpoint.Right),
            };
        }

        private void CaptureCorners(GeometryID hotGeometry)
        {
            float2 position = model.Corners.Get(hotGeometry).Position;
            dragCorners = model.Corners.All
                .Where(corner => corner.Position.Equals(position))
                .ToArray();
        }


        private static float2? CalculateSplit(EdgeData edge, Event @event)
        {
            Line line = edge.Line;
            float2 mousePosition = ProjectOntoGeometry(@event.mousePosition);

            float2 toEnd = line.Left - line.Right;
            float2 toMouse = mousePosition - line.Right;
            float projectedLengthSq = math.dot(toMouse, toEnd);
            float normalizedProjection = math.saturate(projectedLengthSq / math.lengthsq(toEnd));
            bool isOnEdge = normalizedProjection is > 0 and < 1;
            return isOnEdge
                ? math.lerp(line.Right, line.Left, normalizedProjection)
                : null;
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
