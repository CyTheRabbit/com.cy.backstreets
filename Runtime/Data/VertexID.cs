using System;

namespace Backstreets.Data
{
    [Serializable]
    public struct VertexID : IEquatable<VertexID>
    {
        public int contourIndex;
        public int vertexIndex;


        public VertexID(int contourIndex, int vertexIndex)
        {
            this.contourIndex = contourIndex;
            this.vertexIndex = vertexIndex;
        }


        public bool Equals(VertexID other) =>
            contourIndex == other.contourIndex && vertexIndex == other.vertexIndex;

        public override bool Equals(object obj) => obj is VertexID other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(contourIndex, vertexIndex);


        public static readonly VertexID None = new(contourIndex: -1, vertexIndex: -1);
    }
}
