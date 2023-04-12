using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;

namespace Backstreets.FOV.Geometry
{
    [DebuggerDisplay("({End}, {Angle}) {Edge}")]
    internal struct Corner
    {
        public Corner(Line edge, int edgeIndex, Endpoint end)
        {
            Edge = edge;
            EdgeIndex = edgeIndex;
            End = end;
            Angle = LineMath.Angle(end switch
            {
                Endpoint.Right => edge.Right,
                Endpoint.Left => edge.Left,
                _ => throw new ArgumentOutOfRangeException(nameof(end), end, null)
            });
        }

        public Line Edge;
        public int EdgeIndex;
        public float Angle; // TODO: Test if tangent suits better than angle
        public Endpoint End;

        public readonly float2 Position => End switch
        {
            Endpoint.Right => Edge.Right,
            Endpoint.Left => Edge.Left,
            _ => throw new ArgumentOutOfRangeException()
        };


        public enum Endpoint : byte
        {
            Right = 0,
            Left = 1,
        }

        public readonly struct CompareByAngle : IComparer<Corner>
        {
            public int Compare(Corner x, Corner y) => 
                x.Angle.CompareTo(y.Angle) is var angleDifference and not 0 ? angleDifference 
                    : ((byte)x.End).CompareTo((byte)y.End);
        }

        /// <remarks>
        /// This comparer is not transitive, therefore it should not be used to sort corners.
        /// </remarks>
        public readonly struct CompareByAngleCyclic : IComparer<Corner>
        {
            public int Compare(Corner x, Corner y) =>
                LineMath.CompareAngleCyclic(x.Angle, y.Angle) is var angleDifference and not 0 ? angleDifference 
                    : x.End.CompareTo(y.End);
        }
    }
}