using System;
using Backstreets.Pocket;
using Backstreets.Editor.PocketEditor.View;
using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor
{
    [CustomEditor(typeof(PocketPrefabDetails))]
    public class PocketPrefabDetailsEditor : UnityEditor.Editor, IViewController
    {
        private PocketGeometryView view;
        private PocketPrefabDetails Pocket => (PocketPrefabDetails)target;
        GeometryType IViewController.DrawMask => GeometryType.Everything;
        GeometryType IViewController.PickMask => GeometryType.Everything;


        private void OnEnable()
        {
            view = new PocketGeometryView(Pocket, controller: this);
        }

        private void OnSceneGUI()
        {
            view.Process(Event.current);
        }

        void IViewController.OnViewEvent(Event @event, GeometryID hotGeometry)
        {
            bool isLeftMouseClick = @event is { type: EventType.MouseUp, button: 0 };
            if (hotGeometry is { Type: GeometryType.Portal, ID: var portalID } && isLeftMouseClick)
            {
                int portalIndex = Array.FindIndex(Pocket.Portals, portal => portal.edgeID == portalID);
                PortalSelection.Focus(Pocket, portalIndex);
            }
        }

        [DrawGizmo(GizmoType.NonSelected, typeof(PocketPrefabDetails))]
        public static void DrawGizmo(PocketPrefabDetails pocket, GizmoType gizmoType)
        {
            new PocketGeometryView(pocket) { Palette = DesaturatedPalette }.Draw();
        }

        [DrawGizmo(GizmoType.Active | GizmoType.Selected, typeof(PocketPrefabDetails))]
        public static void DrawGizmoActive(PocketPrefabDetails pocket, GizmoType gizmoType)
        {
            // Workaround: unless drawer for selected gizmos is specified, non-selected gizmos will not activate after
            // lose of focus.
        }

        private static readonly Color LightGrey = Color.Lerp(Color.grey, Color.white, 0.5f);

        private static readonly Palette DesaturatedPalette = new()
        {
            EdgeColor = LightGrey,
            BoundsColor = Color.red,
            HotColor = Color.white,
            PortalColor = Color.grey,
        };
    }
}
