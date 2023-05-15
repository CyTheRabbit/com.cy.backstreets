using System;
using System.Linq;
using Backstreets.Editor.PocketEditor.View;
using Backstreets.Pocket;

namespace Backstreets.Editor.PocketEditor.Model
{
    internal abstract class DataAccess<TData>
    {
        protected PocketPrefabDetails Pocket;
        protected GeometryModel Model;

        protected DataAccess(PocketPrefabDetails pocket, GeometryModel model)
        {
            Pocket = pocket;
            Model = model;
        }


        protected abstract GeometryType SupportedType { get; }

        protected abstract TData[] GetDataCollection();

        protected abstract void SetDataCollection(TData[] collection);

        protected abstract GeometryID GetID(TData data);

        protected abstract void AssignID(GeometryID id, ref TData data);

        protected abstract GeometryID GetNewID();


        public virtual TData Get(GeometryID id)
        {
            AssertGeometryTypeMatches(id);
            TData[] collection = GetDataCollection();
            int index = FindIndex(id, collection);
            return index == NotFound ? throw IDNotFound(id) : collection[index];
        }

        private int FindIndex(GeometryID id, TData[] collection)
        {
            int index = Array.FindIndex(collection, data => GetID(data) == id);
            return index;
        }

        public virtual GeometryID Create(TData data)
        {
            GeometryID id = GetNewID();
            AssignID(id, ref data);
            TData[] collection = GetDataCollection();
            using (Model.RecordChanges($"Create {id}"))
            {
                collection = collection.Append(data).ToArray();
                SetDataCollection(collection);
            }

            return id;
        }

        public virtual GeometryID Update(GeometryID id, TData data)
        {
            AssertGeometryTypeMatches(id);

            GeometryID newID = GetID(data);
            if (id != newID) AssertIDNotUsed(newID);

            TData[] collection = GetDataCollection();
            int index = FindIndex(id, collection);
            if (index == NotFound) throw IDNotFound(id);

            using (Model.RecordChanges($"Update {newID}"))
            {
                collection[index] = data;
                SetDataCollection(collection); // In case collection is a copy of data
            }

            return newID;
        }

        public virtual void Delete(GeometryID id)
        {
            AssertGeometryTypeMatches(id);
            TData[] collection = GetDataCollection();
            using (Model.RecordChanges($"Delete {id}"))
            {
                collection = collection.Where(data => GetID(data) != id).ToArray();
                SetDataCollection(collection);
            }
        }


        private void AssertGeometryTypeMatches(GeometryID id)
        {
            if (id.Type != SupportedType)
            {
                throw new ArgumentOutOfRangeException($"Expected ID of type {SupportedType}, but got {id.Type}");
            }
        }

        private void AssertIDNotUsed(GeometryID id)
        {
            bool idUsed = GetDataCollection().Any(data => GetID(data) == id);
            if (idUsed) throw IDAlreadyUsed(id);
        }


        private const int NotFound = -1;

        private static Exception IDNotFound(GeometryID id) =>
            new ArgumentOutOfRangeException($"There is no geometry with id \"{id}\"");

        private static Exception IDAlreadyUsed(GeometryID id) =>
            new ArgumentOutOfRangeException($"ID \"{id}\" is already used");
    }
}
