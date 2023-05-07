using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.FOVTool
{
    public class AnchorHandle
    {
        private readonly GUIContent icon;
        public Vector2 Position;

        public AnchorHandle(Vector2 position, GUIContent icon)
        {
            Position = position;
            this.icon = icon;
        }

        public bool Update()
        {
            DrawAnchorIcon();

            Vector2 oldPosition = Position;
            Position = Handles.FreeMoveHandle(
                position: Position,
                rotation: Handles.matrix.rotation,
                size: AnchorSize,
                snap: Vector3.zero,
                capFunction: Handles.CircleHandleCap);
            return oldPosition != Position;
        }

        private void DrawAnchorIcon()
        {
            if (icon == null) return;
            if (Event.current is not { type: EventType.Repaint }) return;

            GUIContent content = new(icon);

            Vector2 guiPoint = HandleUtility.WorldToGUIPoint(Position);
            Vector2 size = IconStyle.CalcSize(content);
            Rect position = new(guiPoint - size / 2, size);

            Handles.BeginGUI();
            GUI.Label(position, content, IconStyle);
            Handles.EndGUI();
        }

        private static GUIStyle IconStyle => GUI.skin.label;
        private const float AnchorSize = 0.5f;
    }
}