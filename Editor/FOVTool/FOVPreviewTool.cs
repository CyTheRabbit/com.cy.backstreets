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
        private GUIContent guiToolbarIcon;
        private FOVPreviewController controller = null;

        public override GUIContent toolbarIcon => guiToolbarIcon;
        public GUIContent AnchorIcon { get; private set; }

        private void OnEnable()
        {
            guiToolbarIcon = new GUIContent(anchorIcon, "Visibility Preview");
            AnchorIcon = new GUIContent(anchorIcon);
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