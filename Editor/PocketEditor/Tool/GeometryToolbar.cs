using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.Tool
{
    public class GeometryToolbar
    {
        private readonly GUIContent[] contents;
        private readonly ToolFactory[] factories;
        private readonly Action<IGeometryTool> setter;
        private int selectedIndex;


        public GeometryToolbar(Button[] buttons, Action<IGeometryTool> setter, int startingIndex)
        {
            contents = buttons.Select(button => button.Content).ToArray();
            factories = buttons.Select(button => button.Factory).ToArray();
            this.setter = setter;
            selectedIndex = startingIndex;
        }

        public void Process(Event @event)
        {
            using EditorGUI.ChangeCheckScope check = new();
            selectedIndex = GUILayout.Toolbar(selectedIndex, contents);
            if (check.changed)
            {
                IGeometryTool tool = factories[selectedIndex]();
                setter(tool);
            }
        }


        public delegate IGeometryTool ToolFactory();

        public struct Button
        {
            public GUIContent Content;
            public ToolFactory Factory;
        }
    }
}
