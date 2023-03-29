using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Backstreets.Viewport.Jobs
{
    [BurstCompile]
    internal readonly struct BuildCornersJob : IJobParallelFor
    {
        public BuildCornersJob(
            ViewportSpace space,
            NativeArray<Vector2> input,
            NativeArray<Corner> corners,
            NativeArray<int2> spans)
        {
            this.input = input;
            this.space = space;
            this.corners = corners;
            this.spans = spans;
        }

        [ReadOnly] private readonly NativeArray<Vector2> input;
        [ReadOnly] private readonly NativeArray<int2> spans;
        [ReadOnly] private readonly ViewportSpace space;
        [NativeDisableParallelForRestriction]
        [WriteOnly] private readonly NativeArray<Corner> corners;

        public void Execute(int spanIndex)
        {
            int2 slice = spans[spanIndex];
            NativeSlice<Vector2> source = input.Slice(slice.x, slice.y);
            NativeSlice<Corner> destination = new(corners, slice.x, slice.y);

            for (int i = 0; i < source.Length; i++)
            {
                int prevIndex = Mod(i - 1, source.Length);
                int nextIndex = Mod(i + 1, source.Length);
                destination[i] = space.MakeCorner(
                    vertex: source[i],
                    prev: source[prevIndex],
                    next: source[nextIndex]);
            }

            static int Mod(int num, int radix) => num - radix * Mathf.FloorToInt((float)num / radix); // Note integer division
        }
    }


    internal struct ViewportSegment
    {
        public Vector2 Left;
        public Vector2 Right;
    }
}