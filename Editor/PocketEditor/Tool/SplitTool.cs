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
    internal class SplitTool : IGeometryTool
    {
        public event Action<VertexID> OnSplit;


        private readonly GeometryModel model;
        private readonly EventType splitEvent;
        private readonly int splitButton;


        public SplitTool(
            GeometryModel model,
            EventType splitEvent = EventType.MouseDown,
            int splitButton = 0)
        {
            this.model = model;
            this.splitEvent = splitEvent;
            this.splitButton = splitButton;
        }


        public GeometryType DrawMask => GeometryType.Edge;

        public GeometryType PickMask => GeometryType.Edge;


        public void OnViewEvent(Event @event, GeometryID hotGeometry)
        {
            if (IsSplitEvent(@event) && hotGeometry is { Type: GeometryType.Edge })
            {
                PerformSplit(@event, hotGeometry);
                @event.Use();
            }
        }

        public void Dispose()
        {
        }

        public void OnInspectorGUI()
        {
            GUILayout.Label("Click on edges to split");
        }


        private bool IsSplitEvent(Event @event) => @event.type == splitEvent && @event.button == splitButton;

        private void PerformSplit(Event @event, GeometryID hotGeometry)
        {
            Line edge = model.Edges.Get(hotGeometry);
            if (CalculateSplit(edge, @event) is not { } split) return;

            VertexID newVertex;

            using (model.RecordChanges("Split edge"))
            {
                newVertex = model.Corners.Insert(splitEdge: hotGeometry, split);
            }

            OnSplit?.Invoke(newVertex);
        }


        private static float2? CalculateSplit(Line edge, Event @event)
        {
            float2 mousePosition = ProjectOntoGeometry(@event.mousePosition);

            float2 toEnd = edge.Left - edge.Right;
            float2 toMouse = mousePosition - edge.Right;
            float projectedLengthSq = math.dot(toMouse, toEnd);
            float normalizedProjection = math.saturate(projectedLengthSq / math.lengthsq(toEnd));
            bool isOnEdge = normalizedProjection is > 0 and < 1;
            return isOnEdge
                ? math.lerp(edge.Right, edge.Left, normalizedProjection)
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
