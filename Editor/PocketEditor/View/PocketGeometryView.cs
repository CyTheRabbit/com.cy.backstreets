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

        public PocketGeometryView(PocketPrefabDetails pocket)
        {
            controlID = -1;
            this.pocket = pocket;
            Palette = Palette.Default;
            NearestGeometry = GeometryID.None;
        }

        public Palette Palette { get; set; }
        public GeometryID NearestGeometry { get; private set; }

        public GeometryID Pick(GeometryType mask = GeometryType.Everything)
        {
            (GeometryID Selection, float Distance) best = (GeometryID.None, float.PositiveInfinity);

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

            controlID = GUIUtility.GetControlID(ControlHint, FocusType.Passive);
            HandleUtility.AddControl(controlID, best.Distance);
            bool isNearest = HandleUtility.nearestControl == controlID;
            return NearestGeometry = isNearest ? best.Selection : GeometryID.None;
        }

        public void Draw(GeometryType mask = GeometryType.Everything)
        {
            if ((mask & GeometryType.Edge) != 0) DrawEdges();
            if ((mask & GeometryType.Portal) != 0) DrawPortals();
            if ((mask & GeometryType.Bounds) != 0) DrawBounds();
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
                isHot: id == NearestGeometry && GUIUtility.hotControl == controlID);

        private float GetThickness(GeometryID id) =>
            id == NearestGeometry ? 2 : 1;

        private static readonly int ControlHint = "GeometryView".GetHashCode();

        private static float DistanceToEdge(EdgeData edge) =>
            HandleUtility.DistanceToLine(math.float3(edge.left, 0), math.float3(edge.right, 0));

        private static float DistanceToRectangle(Rect rect)
        {
            Matrix4x4 matrix = Matrix4x4.Translate(-rect.position) * Matrix4x4.Scale(rect.size);
            using var matrixScope = new Handles.DrawingScope(matrix);
            return HandleUtility.DistanceToRectangle(Vector2.one / 2, Quaternion.identity, 1);
        }
    }
}
