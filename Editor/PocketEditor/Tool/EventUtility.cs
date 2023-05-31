using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.Tool
{
    public static class EventUtility
    {
        public static Vector2 GetGeometryPosition(this Event @event) => ProjectOntoGeometry(@event.mousePosition);


        private static Vector2 ProjectOntoGeometry(Vector2 guiPoint)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(guiPoint);
            bool isOnZPlane = ray.origin.z == 0;
            if (isOnZPlane) return ray.origin;

            float distanceToZPlane = ray.origin.z / ray.direction.z;
            return ray.GetPoint(distance: distanceToZPlane);
        }
    }
}
