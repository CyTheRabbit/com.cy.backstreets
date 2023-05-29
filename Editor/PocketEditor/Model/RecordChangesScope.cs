using System;
using Backstreets.Pocket;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Backstreets.Editor.PocketEditor.Model
{
    internal readonly struct RecordChangesScope : IDisposable
    {
        private readonly GeometryModel model;
        private readonly PocketPrefabDetails target;

        public RecordChangesScope(GeometryModel model, PocketPrefabDetails target, string name)
        {
            this.model = model;
            this.target = target;
            Undo.RecordObject(target, name);
        }

        public void Dispose()
        {
            EditorUtility.SetDirty(target);
            target.OnValidate();
            model.UpdateView();
        }
    }
}
