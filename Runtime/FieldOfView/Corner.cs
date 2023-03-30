using System.Collections.Generic;
using Unity.Mathematics;

namespace Backstreets.FieldOfView
{
    internal struct Corner
    {
        public float2 Position;
        public float2 Left;
        public float2 Right;
        public float Angle; // TODO: Test if tangent suits better than angle


        public readonly struct CompareByAngle : IComparer<Corner>
        {
            public int Compare(Corner x, Corner y) => x.Angle.CompareTo(y.Angle);
        }

        /// <remarks>
        /// This comparer is not transitive, therefore it should not be used to sort corners.
        /// </remarks>
        public readonly struct CompareByAngleCyclic : IComparer<Corner>
        {
            public int Compare(Corner x, Corner y) => LineMath.CompareAngleCyclic(x.Angle, y.Angle);
        }
    }
}