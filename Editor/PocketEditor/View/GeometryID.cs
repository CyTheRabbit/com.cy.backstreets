using System;
using Backstreets.Data;
using Backstreets.Editor.PocketEditor.Model;

namespace Backstreets.Editor.PocketEditor.View
{
    public readonly struct GeometryID : IEquatable<GeometryID>
    {
        public readonly GeometryType Type;
        public readonly int ID;
        public readonly int Extra;

        public GeometryID(GeometryType type, int id, int extra = default)
        {
            Type = type;
            ID = id;
            Extra = extra;
        }

        public static GeometryID OfBounds() => new(GeometryType.Bounds, -1);

        public static readonly GeometryID None = new(GeometryType.None, -1);


        #region Equality members

        public bool Equals(GeometryID other) => Type == other.Type && ID == other.ID && Extra == other.Extra;

        public override bool Equals(object obj) => obj is GeometryID other && Equals(other);

        public override int GetHashCode() => HashCode.Combine((int)Type, ID, Extra);

        public static bool operator ==(GeometryID a, GeometryID b) => a.Equals(b);

        public static bool operator !=(GeometryID a, GeometryID b) => !a.Equals(b);

        #endregion

        #region Cast

        public static implicit operator GeometryID(EdgeID edgeID) =>
            new(GeometryType.Edge, edgeID.contourIndex, edgeID.edgeIndex);

        public static implicit operator EdgeID(GeometryID id)
        {
            Validation.AssertGeometryType(id, GeometryType.Edge);
            return new EdgeID(contourIndex: id.ID, edgeIndex: id.Extra);
        }

        public static implicit operator GeometryID(VertexID vertexID) =>
            new(GeometryType.Corner, vertexID.contourIndex, vertexID.vertexIndex);

        public static implicit operator VertexID(GeometryID id)
        {
            Validation.AssertGeometryType(id, GeometryType.Corner);
            return new VertexID(contourIndex: id.ID, vertexIndex: id.Extra);
        }

        public static implicit operator GeometryID(PortalData portal) =>
            new(GeometryType.Portal, portal.edgeID.contourIndex, portal.edgeID.edgeIndex);

        #endregion
    }
}
