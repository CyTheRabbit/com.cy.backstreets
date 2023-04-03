using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV.Jobs
{
    public struct JobPromise<T> : INativeDisposable where T : struct, INativeDisposable
    {
        public JobHandle Handle;
        public T Result;

        public JobPromise(JobHandle handle, T result)
        {
            Handle = handle;
            Result = result;
        }

        public T Complete()
        {
            Handle.Complete();
            return Result;
        }

        public void Dispose() => Result.Dispose(Handle);

        public JobHandle Dispose(JobHandle inputDeps) => Result.Dispose(inputDeps);


        public static JobPromise<T> Complete(T value) => new(handle: default, result: value);

        public static implicit operator JobHandle(JobPromise<T> promise) => promise.Handle;
    }
}