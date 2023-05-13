using Backstreets.Data;
using Backstreets.Editor.PocketEditor.CustomHandles;
using Backstreets.Pocket;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.View
{
    public struct PocketGeometryView
    {
        private readonly PocketPrefabDetails pocket;
        private int controlID;
        private GeometryID hotGeometry;
        private GeometryID nearestGeometry;

        public PocketGeometryView(PocketPrefabDetails pocket)
        {
            controlID = -1;
            this.pocket = pocket;
            Palette = Palette.Default;
            DrawMask = GeometryType.Everything;
            PickMask = GeometryType.Everything;
            hotGeometry = GeometryID.None;
            nearestGeometry = GeometryID.None;
        }

        public Palette Palette { get; set; }
        public GeometryType DrawMask { get; set; }
        public GeometryType PickMask { get; set; }

        public void Process(Event @event, InteractionDelegate onInteraction)
        {
            controlID = GUIUtility.GetControlID(ControlHint, FocusType.Passive);
            bool isHot = GUIUtility.hotControl == controlID;
            bool isNearest = HandleUtility.nearestControl == controlID;
            switch (@event)
            {
                case { type: EventType.MouseMove or EventType.Layout }:
                {
                    (GeometryID ID, float Distance) best = FindNearest(PickMask);
                    HandleUtility.AddControl(controlID, best.Distance);
                    nearestGeometry = best.ID;
                    if (hotGeometry != nearestGeometry) hotGeometry = GeometryID.None;
                    break;
                }
                case { type: EventType.MouseDown, button: 0 or 2 } when isNearest:
                {
                    GUIUtility.hotControl = controlID;
                    hotGeometry = nearestGeometry;
                    onInteraction(@event, hotGeometry);
                    Event.current.Use();
                    break;
                }
                case { type: EventType.MouseUp, button: 0 or 2 } when isHot:
                {
                    onInteraction(@event, hotGeometry);
                    GUIUtility.hotControl = 0;
                    hotGeometry = GeometryID.None;
                    Event.current.Use();
                    break;
                }

                case { type: EventType.Repaint }:
                {
                    Draw();
                    onInteraction(@event, hotGeometry);
                    break;
                }
                case { type: not (EventType.Ignore or EventType.Used) } when isHot:
                {
                    onInteraction(@event, hotGeometry);
                    Event.current.Use();
                    break;
                }
            }
        }

        public void Draw()
        {
            if ((DrawMask & GeometryType.Edge) != 0) DrawEdges();
            if ((DrawMask & GeometryType.Portal) != 0) DrawPortals();
            if ((DrawMask & GeometryType.Bounds) != 0) DrawBounds();
        }


        private void DrawEdges()
        {
            foreach (EdgeData edge in pocket.Edges)
            {
                GeometryID id = GeometryID.Of(edge);
                Color color = GetColor(id);
                float thickness = GetThickness(id);

                using var drawingScope = new Handles.DrawingScope(color);
                Handles.DrawLine((Vector2)edge.right, (Vector2)edge.left, thickness);
            }
        }

        private void DrawPortals()
        {
            foreach (PortalData portal in pocket.Portals)
            {
                if (pocket.FindEdge(portal.edgeID) is not { } portalLine) continue;

                GeometryID id = GeometryID.Of(portal);
                Color color = GetColor(id);
                float thickness = GetThickness(id);
                PortalHandle.Static(portalLine, color, thickness);
            }
        }

        private void DrawBounds()
        {
            Color color = GetColor(GeometryID.OfBounds());
            Handles.DrawSolidRectangleWithOutline(pocket.PocketRect, Color.clear, color);
        }

        private Color GetColor(GeometryID id) =>
            Palette.Get(
                baseColor: Palette.GetBaseColor(id.Type),
                isHot: id == hotGeometry && GUIUtility.hotControl == controlID);

        private float GetThickness(GeometryID id) =>
            id == nearestGeometry && HandleUtility.nearestControl == controlID ? 2 : 1;

        private (GeometryID ID, float Distance) FindNearest(GeometryType mask)
        {
            (GeometryID ID, float Distance) best = (GeometryID.None, float.PositiveInfinity);

            void Contest(GeometryID selection, float distance)
            {
                if (distance <= best.Distance) best = (selection, distance);
            }

            if ((mask & GeometryType.Edge) != 0)
            {
                foreach (EdgeData edge in pocket.Edges)
                {
                    Contest(GeometryID.Of(edge), DistanceToEdge(edge));
                }
            }

            if ((mask & GeometryType.Portal) != 0)
            {
                foreach (PortalData portal in pocket.Portals)
                {
                    if (pocket.FindEdge(portal.edgeID) is not { } portalLine) continue;

                    Contest(GeometryID.Of(portal), PortalHandle.DistanceToPortal(portalLine));
                }
            }

            if ((mask & GeometryType.Bounds) != 0)
            {
                Contest(GeometryID.OfBounds(), DistanceToRectangle(pocket.PocketRect));
            }

            return best;
        }

        private static readonly int ControlHint = "GeometryView".GetHashCode();

        private static float DistanceToEdge(EdgeData edge) =>
            HandleUtility.DistanceToLine(math.float3(edge.left, 0), math.float3(edge.right, 0));

        private static float DistanceToRectangle(Rect rect)
        {
            Matrix4x4 matrix = Matrix4x4.Translate(-rect.position) * Matrix4x4.Scale(rect.size);
            using var matrixScope = new Handles.DrawingScope(matrix);
            return HandleUtility.DistanceToRectangle(Vector2.one / 2, Quaternion.identity, 1);
        }


        public delegate void InteractionDelegate(Event @event, GeometryID hotGeometry);
    }
}
