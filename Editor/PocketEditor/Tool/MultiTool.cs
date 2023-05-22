using System.Linq;
using Backstreets.Editor.PocketEditor.View;
using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.Tool
{
    public class MultiTool : IGeometryTool
    {
        private readonly IGeometryTool[] tools;

        public MultiTool(params IGeometryTool[] tools)
        {
            this.tools = tools;

            DrawMask = tools.Aggregate(GeometryType.None, (mask, tool) => mask | tool.DrawMask);
            PickMask = tools.Aggregate(GeometryType.None, (mask, tool) => mask | tool.PickMask);
        }

        public GeometryType DrawMask { get; }

        public GeometryType PickMask { get; }


        public void OnBeforeView(Event @event)
        {
            foreach (IGeometryTool tool in tools)
            {
                tool.OnBeforeView(@event);
            }
        }

        public void OnViewEvent(Event @event, GeometryID hotGeometry)
        {
            foreach (IGeometryTool tool in tools)
            {
                tool.OnViewEvent(@event, hotGeometry);
            }
        }

        public void Dispose()
        {
            foreach (IGeometryTool tool in tools)
            {
                tool.Dispose();
            }
        }

        public void OnInspectorGUI()
        {
            IGeometryTool lastTool = tools.Last();
            foreach (IGeometryTool tool in tools)
            {
                tool.OnInspectorGUI();
                if (tool != lastTool) EditorGUILayout.Separator();
            }
        }
    }
}
