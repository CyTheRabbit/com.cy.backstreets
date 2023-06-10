using System;

namespace Backstreets.Data
{
    public readonly struct PocketID : IEquatable<PocketID>
    {
        public readonly int ID;

        public PocketID(int id) => ID = id;

        public bool Equals(PocketID other) => ID == other.ID;
        public override bool Equals(object obj) => obj is PocketID other && Equals(other);
        public override int GetHashCode() => ID;
        public static bool operator ==(PocketID x, PocketID y) => x.Equals(y);
        public static bool operator !=(PocketID x, PocketID y) => !x.Equals(y);


        public static readonly PocketID Invalid = new(-1);
    }
}
