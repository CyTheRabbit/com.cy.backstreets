using Backstreets.Data;
using Backstreets.Pocket;
using Editor.PocketEditor.CustomHandles;
using UnityEditor;
using UnityEngine;

namespace Editor.PocketEditor
{
    [CustomEditor(typeof(PocketPrefabDetails))]
    public class PocketPrefabDetailsEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            PocketPrefabDetails pocket = (PocketPrefabDetails)target;
            for (int i = 0; i < pocket.Portals.Length; i++)
            {
                PortalData portal = pocket.Portals[i];
                if (pocket.FindEdge(portal.edgeID) is not { } portalLine) continue;
                if (PortalHandle.Clickable(portalLine, HandleColor, 2f))
                {
                    PortalSelection.Focus(pocket, i);
                }
            }
        }

        [DrawGizmo(GizmoType.NonSelected, typeof(PocketPrefabDetails))]
        public static void DrawGizmo(PocketPrefabDetails pocket, GizmoType gizmoType)
        {
            foreach (PortalData portal in pocket.Portals)
            {
                if (pocket.FindEdge(portal.edgeID) is not { } portalLine) continue;
                PortalHandle.Static(portalLine, InactiveColor, 1f);
            }

            foreach (EdgeData edge in pocket.Edges)
            {
                Handles.DrawLine((Vector2)edge.right, (Vector2)edge.left, 1f);
            }
            
            Handles.DrawSolidRectangleWithOutline(pocket.PocketRect, Color.clear, Color.red);
        }

        [DrawGizmo(GizmoType.Active | GizmoType.Selected, typeof(PocketPrefabDetails))]
        public static void DrawGizmoActive(PocketPrefabDetails pocket, GizmoType gizmoType)
        {
            Handles.DrawSolidRectangleWithOutline(pocket.PocketRect, Color.clear, Color.red);
        }

        private static readonly Color HandleColor = Color.cyan;
        private static readonly Color InactiveColor = Color.blue;
    }
}