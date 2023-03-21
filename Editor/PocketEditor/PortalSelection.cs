using Backstreets.Pocket;
using UnityEditor;
using UnityEngine;

namespace Editor.PocketEditor
{
    public class PortalSelection : ScriptableObject
    {
        [SerializeField] private PocketPrefabDetails owner;
        [SerializeField] private int portalIndex;

        public Vector2 Position
        {
            get => owner.Portals[portalIndex].position;
            set => owner.Portals[portalIndex].position = value;
        }

        public float Rotation
        {
            get => owner.Portals[portalIndex].rotation;
            set => owner.Portals[portalIndex].rotation = value;
        }

        public float Scale
        {
            get => owner.Portals[portalIndex].width;
            set => owner.Portals[portalIndex].width = value;
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