using System.Diagnostics.CodeAnalysis;
using Backstreets.FOV.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Backstreets.FOV.Jobs
{
    [BurstCompile]
    internal struct BuildCornersJob : IJobParallelFor
    {
        public BuildCornersJob(
            FieldOfViewSpace space,
            NativeArray<Line> edges,
            NativeArray<Corner> corners)
        {
            this.edges = edges;
            this.space = space;
            this.corners = corners.Reinterpret<CornerPair>(UnsafeUtility.SizeOf<Corner>());
        }

        [ReadOnly] private readonly NativeArray<Line> edges;
        [ReadOnly] private readonly FieldOfViewSpace space;
        [WriteOnly] private NativeArray<CornerPair> corners;

        public void Execute(int index)
        {
            Line worldEdge = edges[index];
            Line localEdge = AlignAgainstOrigin(space.WorldToViewport(worldEdge));
            corners[index] = new CornerPair(localEdge, index);
        }

        private static Line AlignAgainstOrigin(Line edge) =>
            LineMath.GetOriginDomain(edge) switch
            {
                LineMath.LineDomain.Top => edge.Reverse(),
                _ => edge,
            };
        
        [BurstCompatible]
        [SuppressMessage("ReSharper", "NotAccessedField.Local", 
            Justification = "Used only to produce multiple elements in a single iteration of a job.")]
        private readonly struct CornerPair
        {
            private readonly Corner right;
            private readonly Corner left;

            public CornerPair(Line line, int index)
            {
                right = new Corner(line, index, Corner.Endpoint.Right);
                left = new Corner(line, index, Corner.Endpoint.Left);
            }
        }
    }
}
