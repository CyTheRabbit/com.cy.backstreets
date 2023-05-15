using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Backstreets.Editor.PocketEditor.Model
{
    internal readonly struct RecordChangesScope : IDisposable
    {
        private readonly GeometryModel model;
        private readonly Object target;

        public RecordChangesScope(GeometryModel model, Object target, string name)
        {
            this.model = model;
            this.target = target;
            Undo.RecordObject(target, name);
        }

        public void Dispose()
        {
            EditorUtility.SetDirty(target);
            model.UpdateView();
        }
    }
}