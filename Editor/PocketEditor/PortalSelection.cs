using Backstreets.Pocket;
using UnityEditor;
using UnityEngine;

namespace Editor.PocketEditor
{
    public class PortalSelection : ScriptableObject
    {
        [SerializeField] private PocketPrefabDetails owner;
        [SerializeField] private int portalIndex;

        public Vector2 Left
        {
            get => owner.Portals[portalIndex].left;
            set => owner.Portals[portalIndex].left = value;
        }

        public Vector2 Right
        {
            get => owner.Portals[portalIndex].right;
            set => owner.Portals[portalIndex].right = value;
        }


        public static void Focus(PocketPrefabDetails pocket, int portalIndex)
        {
            PortalSelection selection = CreateInstance<PortalSelection>();
            selection.owner = pocket;
            selection.portalIndex = portalIndex;
            Selection.activeTransform = null;
            Selection.activeObject = selection;
        }
    }
}