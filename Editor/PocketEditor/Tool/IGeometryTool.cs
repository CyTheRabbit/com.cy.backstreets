using System;
using Backstreets.Editor.PocketEditor.View;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.Tool
{
    public interface IGeometryTool : IViewController, IDisposable
    {
        void OnBeforeView(Event @event) { }

        void OnInspectorGUI();
    }
}
