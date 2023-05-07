using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV.Jobs
{
    [BurstCompile]
    public struct CopyQueueJob<T> : IJob where T : unmanaged
    {
        private NativeQueue<T> from;
        [WriteOnly] private NativeQueue<T> to;

        public CopyQueueJob(NativeQueue<T> from, NativeQueue<T> to)
        {
            this.from = from;
            this.to = to;
        }

        public void Execute()
        {
            while (!from.IsEmpty())
            {
                to.Enqueue(from.Dequeue());
            }
        }
    }
}