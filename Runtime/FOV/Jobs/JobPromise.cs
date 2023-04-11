using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Backstreets.FOV.Jobs
{
    public struct JobPromise<T> : INativeDisposable where T : struct, INativeDisposable
    {
        public JobHandle Handle;
        private T result;
        private Status status;

        public JobPromise(JobHandle handle, T result)
        {
            Handle = handle;
            this.result = result;
            status = Status.IsValid | Status.OwnsMemory;
        }

        public bool IsValid => HasStatus(Status.IsValid);

        public bool CanScheduleJobs => HasStatus(Status.OwnsMemory);

        /// <summary>
        /// Value allocated for the result of the promise. Should be used to read buffers size and to pass buffers to
        /// jobs.
        /// </summary>
        public T Result => result;

        public T Complete()
        {
            Handle.Complete();
            status &= ~Status.OwnsMemory;
            return Result;
        }

        public void Dispose()
        {
            if (!HasStatus(Status.OwnsMemory)) return;

            Debug.LogErrorFormat(
                format: "Promise of type {0}<{1}> reached end of its life span, yet memory ownership were not transferred nor memory was disposed.",
                nameof(JobHandle), typeof(T).Name);
            result.Dispose(Handle);
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            if (!HasStatus(Status.OwnsMemory)) return default;

            status &= ~Status.OwnsMemory;
            return result.Dispose(inputDeps);
        }

        /// <inheritdoc cref="Reuse(JobHandle)"/>
        public ReusePromise<T> Reuse() => Reuse(Handle);

        /// <summary>
        /// Call this method to modify promise result after it was completed. Result is expected to be immutable, and
        /// to modify its content one should create a new promise.
        /// </summary>
        /// <param name="after">Handle to jobs that depend on the original content of the promise.</param>
        /// <seealso cref="ReusePromise{T}"/>
        public ReusePromise<T> Reuse(JobHandle after)
        {
            status &= ~Status.OwnsMemory;
            return new ReusePromise<T>(after, result);
        }

        private bool HasStatus(Status flags) => (status & flags) == flags;


        public static JobPromise<T> Complete(T value) => new(handle: default, result: value);

        public static implicit operator JobHandle(JobPromise<T> promise) => promise.Handle;

        [Flags]
        public enum Status : byte
        {
            IsValid = 1,
            OwnsMemory = 2,
        }
    }
}