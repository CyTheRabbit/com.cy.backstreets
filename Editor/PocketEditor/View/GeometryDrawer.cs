using Backstreets.Data;
using Backstreets.Editor.PocketEditor.CustomHandles;
using Backstreets.FOV.Geometry;
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
            foreach (VertexID vertexReference in pocket.Polygon.EnumerateVertexIDs())
            {
                DrawCorner(vertexReference);
            }
        }

        private void DrawCorner(VertexID vertex)
        {
            if (NearestGeometry != vertex) return;

            float2 position = pocket.Polygon[vertex];
            DrawCorner(position, GetColor(vertex), GetThickness(vertex));
        }

        internal static void DrawCorner(float2 position, Color color, float thickness)
        {
            using var drawingScope = new Handles.DrawingScope(color);
            float3 position3D = math.float3(position, 0);
            float radius = HandleUtility.GetHandleSize(position3D) * CornerRadius;
            Handles.DrawWireDisc(position3D, Vector3.back, radius, thickness);
        }

        private void DrawEdges()
        {
            foreach ((EdgeID id, Line edge) in pocket.Polygon.EnumerateEdgesWithIDs())
            {
                Color color = GetColor(id);
                float thickness = GetThickness(id);

                using var drawingScope = new Handles.DrawingScope(color);
                Handles.DrawLine((Vector2)edge.Right, (Vector2)edge.Left, thickness);
            }
        }

        private void DrawPortals()
        {
            foreach (PortalData portal in pocket.Portals)
            {
                if (!pocket.Polygon.TryGet(portal.edgeID, out Line portalLine)) continue;

                Color color = GetColor(portal);
                float thickness = GetThickness(portal);
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


        private const float CornerRadius = 0.05f;
    }
}
