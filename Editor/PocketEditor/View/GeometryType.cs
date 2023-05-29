using System;

namespace Backstreets.Editor.PocketEditor.View
{
    [Flags]
    public enum GeometryType
    {
        Edge = 1 << 0,
        Portal = 1 << 1,
        Bounds = 1 << 2,
        Corner = 1 << 3,
        
        Everything = -1,
        None = 0,
    }
}
