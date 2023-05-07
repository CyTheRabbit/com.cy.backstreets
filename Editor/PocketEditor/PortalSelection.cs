using Backstreets.Pocket;
using UnityEditor;
using UnityEngine;

namespace Editor.PocketEditor
{
    public class PortalSelection : ScriptableObject
    {
        [SerializeField] private PocketPrefabDetails owner;
        [SerializeField] private int portalIndex;

        public int EdgeID
        {
            get => owner.Portals[portalIndex].edgeID;
            set => owner.Portals[portalIndex].edgeID = value;
        }

        public int ExitID
        {
            get => owner.Portals[portalIndex].exitID;
            set => owner.Portals[portalIndex].exitID = value;
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