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

        public static void AssertValidIndex(int index, GeometryID id)
        {
            if (index == -1) throw IDNotFound(id);
        }

        public static void AssertIDNotUsed<T>(T[] array, GeometryID id, Func<T, GeometryID> getID)
        {
            foreach (T item in array)
            {
                if (getID(item) == id) throw IDAlreadyUsed(id);
            }
        }
    }
}
