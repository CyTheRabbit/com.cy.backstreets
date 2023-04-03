using Backstreets.FOV.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Backstreets.FOV.Jobs
{
    [BurstCompile]
    internal readonly struct BuildCornersJob : IJobParallelFor
    {
        public BuildCornersJob(
            FieldOfViewSpace space,
            NativeArray<float2> input,
            NativeArray<Corner> corners,
            NativeArray<int2> spans)
        {
            this.input = input;
            this.space = space;
            this.corners = corners;
            this.spans = spans;
        }

        [ReadOnly] private readonly NativeArray<float2> input;
        [ReadOnly] private readonly NativeArray<int2> spans;
        [ReadOnly] private readonly FieldOfViewSpace space;
        [NativeDisableParallelForRestriction]
        [WriteOnly] private readonly NativeArray<Corner> corners;

        public void Execute(int spanIndex)
        {
            int2 slice = spans[spanIndex];
            NativeSlice<float2> source = input.Slice(slice.x, slice.y);
            NativeSlice<Corner> destination = corners.Slice(slice.x * 2, slice.y * 2);

            for (int index = 0; index < source.Length; index++)
            {
                int nextIndex = Mod(index + 1, source.Length);
                Line worldLine = new(right: source[index], left: source[nextIndex]);
                Line localLine = AlignAgainstOrigin(space.WorldToViewport(worldLine));

                destination[index * 2] = new Corner(localLine, Corner.Endpoint.Right);
                destination[index * 2 + 1] = new Corner(localLine, Corner.Endpoint.Left);
            }

            static int Mod(int num, int radix) => num - radix * (int)math.floor((float)num / radix);
        }

        private static Line AlignAgainstOrigin(Line line) =>
            LineMath.GetOriginDomain(line) switch
            {
                LineMath.LineDomain.Top => line.Reverse(),
                _ => line,
            };
    }
}
