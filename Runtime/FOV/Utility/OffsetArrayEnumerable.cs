using Unity.Collections;

namespace Backstreets.FOV.Utility
{
    internal readonly struct OffsetArrayEnumerable<T> where T : unmanaged
    {
        private readonly NativeArray<T> array;
        private readonly int offset;

        public OffsetArrayEnumerable(NativeArray<T> array, int offset)
        {
            this.array = array;
            this.offset = offset;
        }

        public Enumerator GetEnumerator() => new(array, offset);

        internal struct Enumerator
        {
            private readonly NativeArray<T> array;
            private readonly int offset;
            private int index;


            public Enumerator(NativeArray<T> array, int offset)
            {
                this.array = array;
                this.offset = offset;
                index = -1;
            }

            private bool HasMore => index < array.Length;

            public T Current => array[Wrap(index + offset)];

            public bool MoveNext()
            {
                index++;
                return HasMore;
            }

            private int Wrap(int i) => (i % array.Length + array.Length) % array.Length;
        }
    }

    internal static class OffsetArrayExtensions
    {
        public static OffsetArrayEnumerable<T> Offset<T>(this NativeArray<T> array, int offset) where T : unmanaged
            => new(array, offset);
    }
}