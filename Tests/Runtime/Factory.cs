using Backstreets.Viewport;

namespace Backstreets.Tests
{
    internal static class A
    {
        public static ViewportLine Line(ViewportPoint left, ViewportPoint right) =>
            new() { Left = left, Right = right };

        public static ViewportPoint Point(float x, float y) =>
            new() { XY = { x = x, y = y } };
    }
}