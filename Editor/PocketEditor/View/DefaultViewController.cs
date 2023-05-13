using UnityEngine;

namespace Backstreets.Editor.PocketEditor.View
{
    internal class DefaultViewController : IViewController
    {
        public GeometryType DrawMask => GeometryType.Everything;

        public GeometryType PickMask => GeometryType.Everything;

        public void OnViewEvent(Event @event, GeometryID hotGeometry) { }

        public static readonly DefaultViewController Instance = new();
    }
}
