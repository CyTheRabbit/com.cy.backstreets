using Backstreets.Data;
using DefaultNamespace;
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
                PortalData data = pocket.Portals[i];
                if (PortalHandle.Clickable(in data, HandleColor, 2f))
                {
                    PortalSelection.Focus(pocket, i);
                }
            }
        }

        [DrawGizmo(GizmoType.NonSelected, typeof(PocketPrefabDetails))]
        public static void DrawGizmo(PocketPrefabDetails pocket, GizmoType gizmoType)
        {
            foreach (PortalData data in pocket.Portals)
            {
                PortalHandle.Static(in data, InactiveColor, 1f);
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