using System;
using Backstreets.Editor.PocketEditor.View;

namespace Backstreets.Editor.PocketEditor.Tool
{
    public interface IGeometryTool : IViewController, IDisposable
    {
        void OnInspectorGUI();
    }
}
