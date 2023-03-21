using System;

namespace Backstreets.Viewport
{
    internal ref struct ViewportBuilder
    {
        internal ViewportSpace Space;
        internal ViewportLine[] LinesBuffer;
        internal int Count;
        internal ReadOnlySpan<ViewportLine> Result => LinesBuffer[..Count];
    }
}