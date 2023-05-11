using System;
using Backstreets.Data;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.View
{
    public readonly struct GeometryID : IEquatable<GeometryID>
    {
        public readonly GeometryType Type;
        public readonly int ID;

        private GeometryID(GeometryType type, int id)
        {
            Type = type;
            ID = id;
        }

        public static GeometryID Of(EdgeData edge) => new(GeometryType.Edge, edge.id);

        public static GeometryID Of(PortalData portal) => new(GeometryType.Portal, portal.edgeID);

        public static GeometryID OfBounds() => new(GeometryType.Bounds, -1);

        public static readonly GeometryID None = new(GeometryType.None, -1);

        #region Equality members

        public bool Equals(GeometryID other) => Type == other.Type && ID == other.ID;

        public override bool Equals(object obj) => obj is GeometryID other && Equals(other);

        public override int GetHashCode() => HashCode.Combine((int)Type, ID);

        public static bool operator ==(GeometryID a, GeometryID b) => a.Equals(b);

        public static bool operator !=(GeometryID a, GeometryID b) => !a.Equals(b);

        #endregion
    }
}
