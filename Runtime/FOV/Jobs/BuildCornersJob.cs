using System.Diagnostics.CodeAnalysis;
using Backstreets.FOV.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV.Jobs
{
    [BurstCompile]
    internal struct BuildCornersJob : IJobParallelFor
    {
        public BuildCornersJob(
            FieldOfViewSpace space,
            NativeArray<Line> lines,
            NativeArray<Corner> corners)
        {
            this.lines = lines;
            this.space = space;
            this.corners = corners.Reinterpret<CornerPair>(CornerSize);
        }

        [ReadOnly] private readonly NativeArray<Line> lines;
        [ReadOnly] private readonly FieldOfViewSpace space;
        [WriteOnly] private NativeArray<CornerPair> corners;

        public void Execute(int index)
        {
            Line worldLine = lines[index];
            Line localLine = AlignAgainstOrigin(space.WorldToViewport(worldLine));
            corners[index] = new CornerPair(localLine);
        }

        private static Line AlignAgainstOrigin(Line line) =>
            LineMath.GetOriginDomain(line) switch
            {
                LineMath.LineDomain.Top => line.Reverse(),
                _ => line,
            };

        private const int CornerSize = 16 + 4 + 4;
        
        [BurstCompatible]
        [SuppressMessage("ReSharper", "NotAccessedField.Local", 
            Justification = "This struct is used to make Burst happy. Burst is not happy if you map array elements with different indexes.")]
        private readonly struct CornerPair
        {
            private readonly Corner right;
            private readonly Corner left;

            public CornerPair(Line line)
            {
                right = new Corner(line, Corner.Endpoint.Right);
                left = new Corner(line, Corner.Endpoint.Left);
            }
        }
    }
}
