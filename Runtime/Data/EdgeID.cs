﻿using System;

namespace Backstreets.Data
{
    [Serializable]
    public struct EdgeID : IEquatable<EdgeID>
    {
        public int contourIndex;
        public int edgeIndex;


        public EdgeID(int contourIndex, int edgeIndex)
        {
            this.contourIndex = contourIndex;
            this.edgeIndex = edgeIndex;
        }


        public bool Equals(EdgeID other) => contourIndex == other.contourIndex && edgeIndex == other.edgeIndex;

        public override bool Equals(object obj) => obj is EdgeID other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(contourIndex, edgeIndex);

        public static bool operator ==(EdgeID x, EdgeID y) => x.Equals(y);

        public static bool operator !=(EdgeID x, EdgeID y) => !x.Equals(y);


        public static readonly EdgeID None = new(contourIndex: -1, edgeIndex: -1);
    }
}
