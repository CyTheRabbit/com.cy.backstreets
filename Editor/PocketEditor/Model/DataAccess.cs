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
            Validation.AssertGeometryType(id, SupportedType);
            return Validation.FindItem(GetDataCollection(), id, GetID);
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
            Validation.AssertGeometryType(id, SupportedType);
            TData[] collection = GetDataCollection();
            GeometryID newID = GetID(data);
            if (id != newID) Validation.AssertIDNotUsed(collection, newID, GetID);

            int index = Validation.FindIndex(collection, id, GetID);

            using (Model.RecordChanges($"Update {newID}"))
            {
                collection[index] = data;
                SetDataCollection(collection); // In case collection is a copy of data
            }

            return newID;
        }

        public virtual void Delete(GeometryID id)
        {
            Validation.AssertGeometryType(id, SupportedType);
            TData[] collection = GetDataCollection();
            using (Model.RecordChanges($"Delete {id}"))
            {
                collection = collection.Where(data => GetID(data) != id).ToArray();
                SetDataCollection(collection);
            }
        }
    }
}
