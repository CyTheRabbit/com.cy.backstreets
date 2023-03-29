using Backstreets.FieldOfView;
using UnityEngine;

namespace Backstreets.Tests
{
    internal static class A
    {
        public static Line Line(Vector2 left, Vector2 right) => new() { Left = left, Right = right };

        public static Vector2 Point(float x, float y) => new() { x = x, y = y };
    }
}