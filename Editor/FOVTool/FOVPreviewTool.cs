using Backstreets.Data;
using Backstreets.Pocket;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Backstreets.Editor.FOVTool
{
    [EditorTool("Visibility Preview", componentToolTarget: typeof(PocketPrefabDetails))]
    public class FOVPreviewTool : EditorTool
    {
        [SerializeField] private Texture2D anchorIcon;
        private GUIContent icon;
        private FOVPreviewController controller = null;

        public override GUIContent toolbarIcon => icon;
        public GUIContent AnchorIcon => icon;

        private void OnEnable()
        {
            icon = new GUIContent(anchorIcon, "Visibility Preview");
        }

        public override void OnActivated()
        {
            PocketPrefabDetails targetComponent = (PocketPrefabDetails)target;
            Vector3 position = targetComponent.transform.position;
            PocketID pocket = targetComponent.PocketID;
            Scene scene = targetComponent.gameObject.scene;

            controller = new FOVPreviewController(position, pocket, scene, this);
        }

        public override void OnToolGUI(EditorWindow window)
        {
            controller?.Update();
        }

        public override void OnWillBeDeactivated()
        {
            controller?.Dispose();
            controller = null;
        }
    }
}