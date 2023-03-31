using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Backstreets.FOV.Geometry.LineMath;

namespace Backstreets.FOV.Geometry
{
    [BurstCompatible]
    public struct LineOfSight : INativeDisposable
    {
        // It might be more efficient to use a linked list.
        private NativeList<Line> obstacles;
        private readonly CompareObstacleDistance comparer;

        public LineOfSight(int capacity)
        {
            obstacles = new NativeList<Line>(capacity, Allocator.TempJob);
            comparer = new CompareObstacleDistance();
        }

        public float2 Raycast(float2 ray)
        {
            if (obstacles.IsEmpty) return default;
            Line obstacle = obstacles[0];
            return ProjectFromOrigin(obstacle, ray) ?? throw new Exception();
        }

        public UpdateReport AddObstacle(Line insert)
        {
            int insertIndex;
            for (insertIndex = 0; insertIndex < obstacles.Length; insertIndex++)
            {
                if (comparer.Compare(insert, obstacles[insertIndex]) < 0) break;
            }

            if (insertIndex == obstacles.Length)
            {
                obstacles.Add(insert);
            }
            else
            {
                obstacles.InsertRangeWithBeginEnd(insertIndex, insertIndex + 1);
                obstacles[insertIndex] = insert;
            }

            return new UpdateReport
            {
                ClosestObstacleChanged = insertIndex == 0,
            };
        }

        public UpdateReport RemoveObstacle(Line obstacle)
        {
            int index = obstacles.IndexOf(obstacle);
            if (index < 0) throw new ArgumentOutOfRangeException();

            obstacles.RemoveAt(index);
            return new UpdateReport
            {
                ClosestObstacleChanged = index == 0
            };
        }

        public void Dispose() => obstacles.Dispose();

        public JobHandle Dispose(JobHandle inputDeps) => obstacles.Dispose(inputDeps);


        private readonly struct CompareObstacleDistance : IComparer<Line>
        {
            public int Compare(Line x, Line y)
            {
                return SolveFor(x, y) ?? -SolveFor(y, x) ?? 0;

                static int? SolveFor(Line main, Line other)
                {
                    LineDomain leftDomain = GetDomain(main, other.Left);
                    LineDomain rightDomain = GetDomain(main, other.Right);
                    LineDomain otherDomain = Combine(leftDomain, rightDomain);
                    LineDomain originDomain = GetOriginDomain(main);
                    return otherDomain switch
                    {
                        LineDomain.Both => null,
                        LineDomain.Line => 0,
                        _ => otherDomain == originDomain ? 1 : -1,
                    };
                }
            }
        }

        public struct UpdateReport
        {
            public bool ClosestObstacleChanged;


            public static UpdateReport operator +(UpdateReport x, UpdateReport y) =>
                new()
                {
                    ClosestObstacleChanged = x.ClosestObstacleChanged || y.ClosestObstacleChanged,
                };
        }
    }
}