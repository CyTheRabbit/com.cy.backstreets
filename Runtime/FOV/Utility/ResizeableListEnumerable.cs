using Unity.Collections;

namespace Backstreets.FOV.Utility
{
    /// <summary>
    /// Allows to enumerate <see cref="NativeList{T}"/> while filling it with elements. Default enumerator gets
    /// underlying array pointer and iterates over it. However, when list gets resized, array pointer gets replaced.
    /// </summary>
    public readonly struct ResizeableListEnumerable<T> where T : unmanaged
    {
        private readonly NativeList<T> list;

        public ResizeableListEnumerable(NativeList<T> list) => this.list = list;

        public Enumerator GetEnumerator() => new(list);

        public struct Enumerator
        {
            private readonly NativeList<T> list;
            private int index;

            public Enumerator(NativeList<T> list)
            {
                this.list = list;
                index = -1;
            }

            public T Current => list[index];
            public bool MoveNext() => ++index < list.Length;
            public void Reset() => index = -1;
        }
    }

    public static class ResizeableListExtensions
    {
        /// <inheritdoc cref="ResizeableListEnumerable{T}"/>
        public static ResizeableListEnumerable<T> EnumerateResizeable<T>(this NativeList<T> list) where T : unmanaged => 
            new(list);
    }
}