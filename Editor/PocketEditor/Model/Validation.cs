using System;
using Backstreets.Editor.PocketEditor.View;

namespace Backstreets.Editor.PocketEditor.Model
{
    internal static class Validation
    {
        public static Exception IDNotFound(GeometryID id) =>
            new ArgumentOutOfRangeException($"There is no geometry with id \"{id}\"");

        public static Exception IDAlreadyUsed(GeometryID id) =>
            new ArgumentOutOfRangeException($"ID \"{id}\" is already used");

        public static void AssertGeometryType(GeometryID id, GeometryType type)
        {
            if (id.Type != type)
            {
                throw new ArgumentOutOfRangeException($"Expected ID of type {type}, but got {id.Type}");
            }
        }

        public static int FindIndex<T>(T[] array, GeometryID id, Func<T, GeometryID> getID)
        {
            int index = Array.FindIndex(array, item => getID(item) == id);
            if (index == -1) throw IDNotFound(id);
            return index;
        }

        public static T FindItem<T>(T[] array, GeometryID id, Func<T, GeometryID> getID)
        {
            int index = Array.FindIndex(array, item => getID(item) == id);
            if (index == -1) throw IDNotFound(id);
            return array[index];
        }

        public static void AssertIDNotUsed<T>(T[] array, GeometryID id, Func<T, GeometryID> getID)
        {
            int index = Array.FindIndex(array, item => getID(item) == id);
            if (index != -1) throw IDNotFound(id);
        }
    }
}
