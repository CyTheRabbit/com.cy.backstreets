using Backstreets.Data;
using Backstreets.Editor.PocketEditor.CustomHandles;
using Backstreets.Editor.PocketEditor.Model;
using Backstreets.Pocket;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.View
{
    public struct GeometryDrawer
    {
        private readonly PocketPrefabDetails pocket;

        public GeometryDrawer(PocketPrefabDetails pocket)
        {
            this.pocket = pocket;
            Palette = Palette.Default;
            NearestGeometry = GeometryID.None;
            HotGeometry = GeometryID.None;
        }

        public Palette Palette { get; set; }

        public GeometryID NearestGeometry { get; set; }

        public GeometryID HotGeometry { get; set; }


        public void Draw(GeometryType mask)
        {
            if ((mask & GeometryType.Edge) != 0) DrawEdges();
            if ((mask & GeometryType.Portal) != 0) DrawPortals();
            if ((mask & GeometryType.Bounds) != 0) DrawBounds();
            if ((mask & GeometryType.Corner) != 0) DrawCorners();
        }


        private void DrawCorners()
        {
            foreach (EdgeData edge in pocket.Edges)
            {
                DrawCorner(new CornerData(edge, CornerData.Endpoint.Right));
                DrawCorner(new CornerData(edge, CornerData.Endpoint.Left));
            }
        }

        private void DrawCorner(CornerData corner)
        {
            GeometryID id = GeometryID.Of(corner);
            if (NearestGeometry != id) return;

            Color color = GetColor(id);
            float thickness = GetThickness(id);
            using var drawingScope = new Handles.DrawingScope(color);
            Handles.DrawWireDisc(math.float3(corner.Position, 0), Vector3.back, CornerRadius, thickness);
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
                isHot: HotGeometry == id);

        private float GetThickness(GeometryID id) =>
            NearestGeometry == id ? 2 : 1;


        private const float CornerRadius = 0.025f;
    }
}
