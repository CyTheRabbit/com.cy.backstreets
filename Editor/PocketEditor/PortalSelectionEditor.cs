using UnityEditor;
using UnityEngine;

namespace Editor.PocketEditor
{
    [CustomEditor(typeof(PortalSelection))]
    public class PortalSelectionEditor : UnityEditor.Editor
    {
        private GUIStyle headerStyle;

        private PortalSelection Selection => target as PortalSelection;

        protected override void OnHeaderGUI()
        {
            ReloadStyles();

            using EditorGUILayout.HorizontalScope horizontal = new(headerStyle);
            Texture2D icon = AssetPreview.GetMiniTypeThumbnail(typeof(Grid));
            GUILayout.Label(icon, GUILayout.Width(IconSize), GUILayout.Height(IconSize));

            using EditorGUILayout.VerticalScope vertical = new();
            using EditorGUI.ChangeCheckScope check = new();

            GUILayout.Label(Label);
            Selection.Left = EditorGUILayout.Vector2Field(GUIContent.none, Selection.Left);
            Selection.Right = EditorGUILayout.Vector2Field(GUIContent.none, Selection.Right);
        }

        private void ReloadStyles()
        {
            headerStyle ??= FindStyle(HeaderStyleName);

            static GUIStyle FindStyle(string style) =>
                GUI.skin.FindStyle(style) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(style);
        }

        private const float IconSize = 32f;
        private const string HeaderStyleName = "In BigTitle";
        private static readonly GUIContent Label = new("Portal");
    }
}