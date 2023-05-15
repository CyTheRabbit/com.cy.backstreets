using Backstreets.Editor.PocketEditor.Model;
using Backstreets.Editor.PocketEditor.Tool;
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
        private GeometryModel model;
        private IGeometryTool activeTool;

        private PocketPrefabDetails Pocket => (PocketPrefabDetails)target;
        GeometryType IViewController.DrawMask => activeTool?.DrawMask ?? GeometryType.Everything;
        GeometryType IViewController.PickMask => activeTool?.PickMask ?? GeometryType.Everything;


        private void OnEnable()
        {
            view = new PocketGeometryView(Pocket, controller: this);
            model = new GeometryModel(Pocket, updateViewAction: Repaint);
            activeTool = new SelectionTool(model);
        }

        public override void OnInspectorGUI()
        {
            if (activeTool != null)
            {
                using var box = new GUILayout.VerticalScope(GUI.skin.box, GUILayout.ExpandWidth(true));
                activeTool.OnInspectorGUI();
            }

            base.OnInspectorGUI();
        }

        private void OnSceneGUI()
        {
            view.Process(Event.current);
        }

        void IViewController.OnViewEvent(Event @event, GeometryID hotGeometry) =>
            activeTool?.OnViewEvent(@event, hotGeometry);

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
