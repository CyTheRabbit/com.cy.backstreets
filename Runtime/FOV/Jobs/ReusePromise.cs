using System;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV.Jobs
{
    /// <summary>
    /// Allows to reuse resources of another <see cref="JobPromise{T}"/> for new values. Owner of this promise
    /// becomes responsible for disposing native resources or delegating it somewhere else.
    /// Structurally its the same as <see cref="JobPromise{T}"/>, but the value of <see cref="ReusePromise{T}"/> can
    /// be modified inside jobs without risk of race condition.
    /// </summary>
    public struct ReusePromise<T> where T : struct, INativeDisposable
    {
        public JobHandle Handle;
        public T Value;

        public ReusePromise(JobHandle handle, T value)
        {
            Handle = handle;
            Value = value;
        }

        public void Dispose() => Value.Dispose();

        public static implicit operator JobHandle(ReusePromise<T> promise) => promise.Handle;
    }
}