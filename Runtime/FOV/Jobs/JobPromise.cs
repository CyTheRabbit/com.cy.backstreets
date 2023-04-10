using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV.Jobs
{
    public struct JobPromise<T> : INativeDisposable where T : struct, INativeDisposable
    {
        public JobHandle Handle;
        private T result;
        private bool ownsResult;

        public JobPromise(JobHandle handle, T result)
        {
            Handle = handle;
            this.result = result;
            ownsResult = true;
        }

        public T Result => result;

        public T Complete()
        {
            Handle.Complete();
            ownsResult = false;
            return Result;
        }

        public void Dispose()
        {
            if (!ownsResult) return;
            result.Dispose(Handle);
        }

        public JobHandle Dispose(JobHandle inputDeps) => ownsResult ? result.Dispose(inputDeps) : default;

        public ReusePromise<T> Reuse() => Reuse(Handle);

        /// <summary>
        /// Call this method to modify promise result after it was completed. Result is expected to be immutable, and
        /// to modify its content one should create a new promise.
        /// </summary>
        /// <param name="after">Handle to jobs that depend on the original content of the promise.</param>
        /// <seealso cref="ReusePromise{T}"/>
        public ReusePromise<T> Reuse(JobHandle after)
        {
            ownsResult = false;
            return new ReusePromise<T>(after, result);
        }

        public static JobPromise<T> Complete(T value) => new(handle: default, result: value);

        public static implicit operator JobHandle(JobPromise<T> promise) => promise.Handle;
    }
}