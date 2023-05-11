using System;
using Backstreets.Pocket;
using Backstreets.Editor.PocketEditor.View;
using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor
{
    [CustomEditor(typeof(PocketPrefabDetails))]
    public class PocketPrefabDetailsEditor : UnityEditor.Editor
    {
        private PocketGeometryView view;
        private PocketPrefabDetails Pocket => (PocketPrefabDetails)target;


        private void OnEnable()
        {
            view = new PocketGeometryView(Pocket);
        }

        private void OnSceneGUI()
        {
            GeometryID pick = view.Pick();
            view.Draw(mask: GeometryType.Everything);

            bool isLeftMouseClick = Event.current is { type: EventType.MouseDown, button: 0 };
            if (pick is { Type: GeometryType.Portal, ID: var portalID } && isLeftMouseClick)
            {
                int portalIndex = Array.FindIndex(Pocket.Portals, portal => portal.edgeID == portalID);
                PortalSelection.Focus(Pocket, portalIndex);
            }
        }

        [DrawGizmo(GizmoType.NonSelected, typeof(PocketPrefabDetails))]
        public static void DrawGizmo(PocketPrefabDetails pocket, GizmoType gizmoType)
        {
            new PocketGeometryView(pocket).Draw();
        }

        [DrawGizmo(GizmoType.Active | GizmoType.Selected, typeof(PocketPrefabDetails))]
        public static void DrawGizmoActive(PocketPrefabDetails pocket, GizmoType gizmoType)
        {
            // Workaround: unless drawer for selected gizmos is specified, non-selected gizmos will not activate after
            // lose of focus.
        }
    }
}
