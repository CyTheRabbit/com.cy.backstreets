using Backstreets.Pocket;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor.PocketEditor
{
    public class PocketStage : PreviewSceneStage
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private Grid _grid;
        
        protected override GUIContent CreateHeaderContent() => new(_prefab.name);

        protected override bool OnOpenStage()
        {
            if (_prefab == null) return false;
            if (!_prefab.TryGetComponent(out PocketPrefabDetails pocket)) return false;
            if (!base.OnOpenStage()) return false;

            SceneManager.SetActiveScene(scene);

            GameObject gridGameObject = new("Grid", typeof(Grid))
            {
                hideFlags = HideFlags.NotEditable,
                transform =
                {
                    hideFlags = HideFlags.HideInInspector
                }
            };
            _grid = gridGameObject.GetComponent<Grid>();

            return true;
        }

        protected override void OnCloseStage()
        {
            base.OnCloseStage();
        }
    }
}