using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV.Jobs
{
    [BurstCompile]
    internal readonly struct CopyArrayJob<T> : IJob
        where T : struct
    {
        public CopyArrayJob(NativeArray<T> source, NativeArray<T> destination)
        {
            this.source = source;
            this.destination = destination;
        }

        [ReadOnly] private readonly NativeArray<T> source;
        [WriteOnly] private readonly NativeArray<T> destination;

        public void Execute()
        {
            NativeArray<T>.Copy(source, destination);
        }
    }
}