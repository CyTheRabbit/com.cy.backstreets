using System;
using Unity.Collections;
using UnityEngine;

namespace Backstreets.FOV.Sandbox
{
    public static class FanMeshColoring
    {
        public static void SetColor(Mesh mesh, Palette palette)
        {
            NativeArray<Color> colors = new(mesh.vertexCount, Allocator.Temp);

            colors[0] = palette.origin;
            for (int i = 1; i < colors.Length; i++)
            {
                int triangleIndex = (i - 1) / 2;
                bool isEven = (triangleIndex & 1) == 0;
                colors[i] = isEven ? palette.even : palette.odd;
            }

            mesh.SetColors(colors);
            colors.Dispose();
        }

        [Serializable]
        public struct Palette
        {
            public Color origin;
            public Color odd;
            public Color even;

            public Palette Alpha(float alpha) => new()
            {
                origin = origin * alpha,
                odd = odd * alpha,
                even = even * alpha
            };
        }
    }
}