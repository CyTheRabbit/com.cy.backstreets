using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.Tool.Move
{
    internal struct SelectionBox
    {
        private Vector2 start;
        private Vector2 end;
        public bool IsDragging;

        public Rect WorldRect => new(
                position: new Vector2(
                    x: math.min(start.x, end.x),
                    y: math.min(start.y, end.y)),
                size:new Vector2(
                    x: math.abs(start.x - end.x),
                    y: math.abs(start.y - end.y)));

        private static readonly int SelectHint = "Select".GetHashCode();
        private static readonly Color SelectionFaceColor = new(0.5f, 0.675f, 1, 0.05f);
        private static readonly Color SelectionOutlineColor = new(0.5f, 0.675f, 1);

        public static implicit operator Rect(SelectionBox box) => box.WorldRect;

        public static SelectionBox Handle(SelectionBox box, Event @event)
        {
            int controlID = GUIUtility.GetControlID(SelectHint, FocusType.Passive);
            bool isNearest = HandleUtility.nearestControl == controlID;
            bool isHot = GUIUtility.hotControl == controlID;

            switch (@event)
            {
                case { type: EventType.MouseDown, button: 0 } when isNearest:
                {
                    Vector2 position = @event.GetGeometryPosition();
                    box = new SelectionBox { start = position, end = position, IsDragging = true };
                    GUIUtility.hotControl = controlID;
                    GUI.changed = true;
                    @event.Use();
                    break;
                }
                case { type: EventType.MouseDrag, button: 0 } when isHot:
                {
                    box.end = @event.GetGeometryPosition();
                    GUI.changed = true;
                    @event.Use();
                    break;
                }
                case { type: EventType.MouseUp, button: 0 } when isHot:
                {
                    box.IsDragging = false;
                    GUIUtility.hotControl = 0;
                    GUI.changed = true;
                    @event.Use();
                    break;
                }
                case { type: EventType.Layout or EventType.MouseMove }:
                {
                    HandleUtility.AddDefaultControl(controlID);
                    break;
                }
                case { type: EventType.Repaint } when isHot:
                {
                    Handles.DrawSolidRectangleWithOutline(box, SelectionFaceColor, SelectionOutlineColor);
                    break;
                }
            }

            return box;
        }
    }
}
