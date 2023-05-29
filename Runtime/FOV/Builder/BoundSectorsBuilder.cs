using Backstreets.Data;
using Backstreets.FOV.Geometry;
using Backstreets.FOV.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Backstreets.FOV.Geometry.LineMath;

namespace Backstreets.FOV.Builder
{
    internal static class BoundSectorsBuilder<TRecorder> where TRecorder : struct, ISweepSectorRecorder
    {
        public static JobHandle SweepSectors(
            TRecorder recorder,
            PocketID pocketID,
            in JobPromise<FarBounds> entranceBounds,
            in JobPromise<FarBounds> exitBounds)
        {
            NativeQueue<Bound> nearBounds = entranceBounds.Result.Bounds;
            NativeQueue<Bound> farBounds = exitBounds.Result.Bounds;
            JobHandle inputDeps = JobHandle.CombineDependencies(entranceBounds, exitBounds);
            return new CombineBoundsJob(recorder, nearBounds, farBounds, pocketID).Schedule(inputDeps);
        }

        public static JobHandle SweepFromOrigin(
            TRecorder recorder,
            PocketID pocketID,
            in JobPromise<FarBounds> bounds)
        {
            NativeQueue<Bound> farBounds = bounds.Result.Bounds;
            return new SingleBoundJob(recorder, farBounds, pocketID).Schedule(bounds);
        }

        [BurstCompile]
        private struct CombineBoundsJob : IJob
        {
            [ReadOnly] private NativeQueue<Bound> nearBoundsQueue;
            [ReadOnly] private NativeQueue<Bound> farBoundsQueue;
            private TRecorder recorder;
            private readonly PocketID pocketID;

            public CombineBoundsJob(
                TRecorder recorder,
                NativeQueue<Bound> nearBoundsQueue,
                NativeQueue<Bound> farBoundsQueue,
                PocketID pocketID)
            {
                this.nearBoundsQueue = nearBoundsQueue;
                this.farBoundsQueue = farBoundsQueue;
                this.recorder = recorder;
                this.pocketID = pocketID;
            }

            public void Execute()
            {
                using NativeArray<Bound> nearBounds = nearBoundsQueue.ToArray(Allocator.Temp);
                using NativeArray<Bound> farBounds = farBoundsQueue.ToArray(Allocator.Temp);

                if (nearBounds.Length == 0 || farBounds.Length == 0) return;

                SimpleSweep farSweep = new(farBounds);
                if (!farSweep.MoveNext()) return;

                foreach (Bound nearBound in nearBounds)
                {
                    float nearRightAngle = Angle(nearBound.Right, AnglePreferences.PreferNegative);
                    float nearLeftAngle = Angle(nearBound.Left);

                    // Advance til start of near bound
                    while (farSweep.HasMore && Angle(farSweep.Current.Left) <= nearRightAngle)
                    {
                        farSweep.MoveNext();
                    }
                    if (farSweep.IsComplete) break;

                    while (farSweep.HasMore && Angle(farSweep.Current.Left) < nearLeftAngle)
                    {
                        TryRecord(nearBound, farSweep.Current);
                        farSweep.MoveNext();
                    }

                    if (farSweep.IsComplete) break;
                    TryRecord(nearBound, farSweep.Current);
                }
            }

            private void TryRecord(in Bound near, in Bound far)
            {
                if (Intersect(near, far) is not { near: var nearCut, far: var farCut }) return;
                recorder.Record(new BoundSector(nearCut, farCut, pocketID));
            }

            private static (Bound near, Bound far)? Intersect(Bound near, Bound far)
            {
                if (GetDomain(near.Right, far.Left) is RayDomain.Right ||
                    GetDomain(near.Left, far.Right) is RayDomain.Left) return null;

                float2 rightRay = GetDomain(near.Right, far.Right) is RayDomain.Left ? far.Right : near.Right;
                float2 leftRay = GetDomain(near.Left, far.Left) is RayDomain.Right ? far.Left : near.Left;

                near.Right = ProjectFromOrigin(near, rightRay) ?? near.Right;
                near.Left = ProjectFromOrigin(near, leftRay) ?? near.Left;
                far.Right = ProjectFromOrigin(far, rightRay) ?? far.Right;
                far.Left = ProjectFromOrigin(far, leftRay) ?? far.Left;
                return (near, far);
            }
        }

        [BurstCompile]
        private struct SingleBoundJob : IJob
        {
            [ReadOnly] private NativeQueue<Bound> farBoundsQueue;
            private readonly PocketID pocket;
            private TRecorder recorder;

            public SingleBoundJob(TRecorder recorder, NativeQueue<Bound> farBounds, PocketID pocketID)
            {
                farBoundsQueue = farBounds;
                pocket = pocketID;
                this.recorder = recorder;
            }

            public void Execute()
            {
                using NativeArray<Bound> farBounds = farBoundsQueue.ToArray(Allocator.Temp);
                SimpleSweep farSweep = new(farBounds);
                while (farSweep.MoveNext())
                {
                    recorder.Record(new BoundSector(default, farSweep.Current, pocket));
                }
            }
        }

        /// <summary>
        /// Sweep bounds assuming no bounds overlap over each other.
        /// </summary>
        private struct SimpleSweep
        {
            private NativeArray<Bound>.Enumerator enumerator;
            public bool HasMore;

            public SimpleSweep(NativeArray<Bound> bounds)
            {
                enumerator = bounds.GetEnumerator();
                HasMore = true;
            }

            public Bound Current => enumerator.Current;
            public bool IsComplete => !HasMore;

            public bool MoveNext() => HasMore = enumerator.MoveNext();

            public bool AdvanceUntilAngle(float angle)
            {
                while (HasMore && Angle(Current.Left) <= angle) MoveNext();
                return HasMore;
            }
        }
    }

    public interface ISweepSectorRecorder
    {
        void Record(BoundSector sector);
    }
}
