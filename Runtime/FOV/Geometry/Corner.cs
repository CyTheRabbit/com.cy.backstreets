using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;

namespace Backstreets.FOV.Geometry
{
    [DebuggerDisplay("({End}, {Angle}) {Line}")]
    internal struct Corner
    {
        public Corner(Line line, int lineIndex, Endpoint end)
        {
            Line = line;
            LineIndex = lineIndex;
            End = end;
            Angle = LineMath.Angle(end switch
            {
                Endpoint.Right => line.Right,
                Endpoint.Left => line.Left,
                _ => throw new ArgumentOutOfRangeException(nameof(end), end, null)
            });
        }

        public Line Line;
        public int LineIndex;
        public float Angle; // TODO: Test if tangent suits better than angle
        public Endpoint End;

        public readonly float2 Position => End switch
        {
            Endpoint.Right => Line.Right,
            Endpoint.Left => Line.Left,
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