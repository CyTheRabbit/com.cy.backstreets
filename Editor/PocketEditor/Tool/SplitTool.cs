using System;
using Backstreets.Data;
using Backstreets.Editor.PocketEditor.Model;
using Backstreets.Editor.PocketEditor.View;
using Backstreets.FOV.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.Tool
{
    internal class SplitTool : IGeometryTool
    {
        public event Action<CornerData, CornerData> OnSplit;


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
            EdgeData edge = model.Edges.Get(hotGeometry);
            if (CalculateSplit(edge, @event) is not { } split) return;

            EdgeData rightCut = new() { id = edge.id, right = edge.right, left = split };
            EdgeData leftCut = new() { id = default, right = split, left = edge.left };

            model.Edges.Update(hotGeometry, rightCut);
            model.Edges.Create(ref leftCut);

            OnSplit?.Invoke(
                new CornerData(rightCut, CornerData.Endpoint.Left),
                new CornerData(leftCut, CornerData.Endpoint.Right));
        }


        private static float2? CalculateSplit(EdgeData edge, Event @event)
        {
            Line line = edge.Line;
            float2 mousePosition = @event.GetGeometryPosition();

            float2 toEnd = line.Left - line.Right;
            float2 toMouse = mousePosition - line.Right;
            float projectedLengthSq = math.dot(toMouse, toEnd);
            float normalizedProjection = math.saturate(projectedLengthSq / math.lengthsq(toEnd));
            bool isOnEdge = normalizedProjection is > 0 and < 1;
            return isOnEdge
                ? math.lerp(line.Right, line.Left, normalizedProjection)
                : null;
        }
    }
}
