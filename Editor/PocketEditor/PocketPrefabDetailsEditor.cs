using Backstreets.Editor.PocketEditor.Model;
using Backstreets.Editor.PocketEditor.Tool;
using Backstreets.Editor.PocketEditor.Tool.Move;
using Backstreets.Pocket;
using Backstreets.Editor.PocketEditor.View;
using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor
{
    [CustomEditor(typeof(PocketPrefabDetails))]
    public class PocketPrefabDetailsEditor : UnityEditor.Editor, IViewController
    {
        private GeometryToolbar toolbar;
        private PocketGeometryView view;
        private GeometryModel model;
        private IGeometryTool activeTool;

        private PocketPrefabDetails Pocket => (PocketPrefabDetails)target;
        GeometryType IViewController.DrawMask => activeTool?.DrawMask ?? GeometryType.Everything;
        GeometryType IViewController.PickMask => activeTool?.PickMask ?? GeometryType.None;


        private void OnEnable()
        {
            view = new PocketGeometryView(Pocket, controller: this);
            model = new GeometryModel(Pocket, UpdateView);
            activeTool = null;
            toolbar = new GeometryToolbar(new GeometryToolbar.Button[]
            {
                new() {Content = new GUIContent("Inspect"), Factory = () => new SelectionTool(model)},
                new() {Content = new GUIContent("Move"), Factory = () => new MoveTool(model)},
                new() {Content = new GUIContent("Deform"), Factory = () => new DeformTool(model)},
                new() {Content = new GUIContent("Raw"), Factory = () => null}
            }, SetTool, startingIndex: ^1);
        }

        private void SetTool(IGeometryTool tool)
        {
            activeTool?.Dispose();
            activeTool = tool;
            UpdateView();
        }

        public override void OnInspectorGUI()
        {
            toolbar.Process(Event.current);
            if (activeTool != null)
            {
                using var box = new GUILayout.VerticalScope(GUI.skin.box, GUILayout.ExpandWidth(true));
                activeTool.OnInspectorGUI();
            }
            else
            {
                base.OnInspectorGUI();
            }
        }

        private void OnSceneGUI()
        {
            activeTool?.OnBeforeView(Event.current);
            view.Process(Event.current);
        }

        private void UpdateView()
        {
            Repaint();
            foreach (SceneView sceneView in SceneView.sceneViews)
            {
                sceneView.Repaint();
            }
        }

        void IViewController.OnViewEvent(Event @event, GeometryID hotGeometry) =>
            activeTool?.OnViewEvent(@event, hotGeometry);

        [DrawGizmo(GizmoType.NonSelected, typeof(PocketPrefabDetails))]
        public static void DrawGizmo(PocketPrefabDetails pocket, GizmoType gizmoType)
        {
            new GeometryDrawer(pocket) { Palette = DesaturatedPalette }.Draw(GeometryType.Everything);
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
            CornerColor = LightGrey,
            BoundsColor = Color.red,
            PortalColor = Color.grey,
        };
    }
}
