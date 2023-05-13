using UnityEngine;

namespace Backstreets.Editor.PocketEditor.View
{
    public interface IViewController
    {
        GeometryType DrawMask { get; }

        GeometryType PickMask { get; }

        void OnViewEvent(Event @event, GeometryID hotGeometry);
    }
}
