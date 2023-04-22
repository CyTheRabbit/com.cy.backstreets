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

            for (int i = 0; i < colors.Length; i++)
            {
                int quadIndex = i / 4;
                int indexInQuad = i % 4;
                bool isNearEdge = (indexInQuad & 2) == 0;
                bool isEven = (quadIndex & 1) == 0;
                colors[i] =
                    isNearEdge ? palette.origin :
                    isEven ? palette.even :
                    palette.odd;
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