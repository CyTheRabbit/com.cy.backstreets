using System.Collections.Generic;
using Backstreets.Data;
using Backstreets.Editor.PocketEditor.CustomHandles;
using Backstreets.Editor.PocketEditor.Model;
using Backstreets.Pocket;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.View
{
    public class PocketGeometryView
    {
        private readonly PocketPrefabDetails pocket;
        private readonly IViewController controller;
        private readonly Dictionary<int, GeometryID> controlToGeometry;

        public PocketGeometryView(PocketPrefabDetails pocket, IViewController controller = null)
        {
            controlToGeometry = new Dictionary<int, GeometryID>();
            this.pocket = pocket;
            this.controller = controller ?? DefaultViewController.Instance;
            Palette = Palette.Default;
        }

        public Palette Palette { get; set; }

        public GeometryID NearestGeometry => GetGeometryByControlID(HandleUtility.nearestControl);

        public GeometryID HotGeometry => GetGeometryByControlID(GUIUtility.hotControl);


        public void Process(Event @event)
        {
            bool isHot = controlToGeometry.ContainsKey(GUIUtility.hotControl);
            bool isNearest = controlToGeometry.ContainsKey(HandleUtility.nearestControl);
            switch (@event)
            {
                case { type: EventType.MouseMove or EventType.Layout }:
                {
                    AddControls(controller.PickMask);
                    break;
                }
                case { type: EventType.MouseDown, button: 0 or 2 } when isNearest:
                {
                    GUIUtility.hotControl = HandleUtility.nearestControl;
                    controller.OnViewEvent(@event, HotGeometry);
                    Event.current.Use();
                    break;
                }
                case { type: EventType.MouseUp, button: 0 or 2 } when isHot:
                {
                    controller.OnViewEvent(@event, HotGeometry);
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                    break;
                }

                case { type: EventType.Repaint }:
                {
                    Draw();
                    controller.OnViewEvent(@event, HotGeometry);
                    break;
                }
                case { type: not (EventType.Ignore or EventType.Used) } when isHot:
                {
                    controller.OnViewEvent(@event, HotGeometry);
                    Event.current.Use();
                    break;
                }
            }
        }

        public void Draw()
        {
            GeometryDrawer drawer = new(pocket)
            {
                Palette = Palette,
                NearestGeometry = NearestGeometry,
                HotGeometry = HotGeometry,
            };
            drawer.Draw(controller.DrawMask);
        }


        private void AddControls(GeometryType mask)
        {
            controlToGeometry.Clear();

            if ((mask & GeometryType.Corner) != 0)
            {
                foreach (EdgeData edge in pocket.Edges)
                {
                    CornerData right = new(edge, CornerData.Endpoint.Right);
                    CornerData left = new(edge, CornerData.Endpoint.Left);
                    AddControl(GeometryID.Of(right), DistanceToCorner(right));
                    AddControl(GeometryID.Of(left), DistanceToCorner(left));
                }
            }

            if ((mask & GeometryType.Edge) != 0)
            {
                foreach (EdgeData edge in pocket.Edges)
                {
                    AddControl(GeometryID.Of(edge), DistanceToEdge(edge));
                }
            }

            if ((mask & GeometryType.Portal) != 0)
            {
                foreach (PortalData portal in pocket.Portals)
                {
                    if (pocket.FindEdge(portal.edgeID) is not { } portalLine) continue;

                    AddControl(GeometryID.Of(portal), PortalHandle.DistanceToPortal(portalLine));
                }
            }

            if ((mask & GeometryType.Bounds) != 0)
            {
                AddControl(GeometryID.OfBounds(), DistanceToRectangle(pocket.PocketRect));
            }
        }

        private void AddControl(GeometryID geometryID, float distance)
        {
            int controlID = GUIUtility.GetControlID(hint: geometryID.GetHashCode(), FocusType.Passive);
            controlToGeometry.Add(controlID, geometryID);
            HandleUtility.AddControl(controlID, distance);
        }

        private GeometryID GetGeometryByControlID(int controlID) =>
            controlToGeometry.TryGetValue(controlID, out GeometryID geometry) ? geometry : GeometryID.None;


        private const float CornerRadius = 0.025f;


        private static float DistanceToEdge(EdgeData edge) =>
            HandleUtility.DistanceToLine(math.float3(edge.left, 0), math.float3(edge.right, 0));

        private static float DistanceToRectangle(Rect rect)
        {
            Matrix4x4 matrix = Matrix4x4.Translate(-rect.position) * Matrix4x4.Scale(rect.size);
            using var matrixScope = new Handles.DrawingScope(matrix);
            return HandleUtility.DistanceToRectangle(Vector2.one / 2, Quaternion.identity, 1);
        }

        private static float DistanceToCorner(CornerData corner) =>
            HandleUtility.DistanceToCircle(math.float3(corner.Position, 0), CornerRadius);
    }
}
