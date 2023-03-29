using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static Backstreets.Viewport.ViewportMath;

namespace Backstreets.Viewport
{
    [BurstCompatible]
    public struct LineOfSight : INativeDisposable
    {
        // It might be more efficient to use a linked list.
        private NativeList<Obstacle> obstacles;
        private readonly CompareObstacleDistance comparer;

        public LineOfSight(int capacity)
        {
            obstacles = new NativeList<Obstacle>(capacity, Allocator.TempJob);
            comparer = new CompareObstacleDistance();
        }

        public Obstacle GetClosestObstacle(Vector2 ray)
        {
            return obstacles[0];
        }

        public Vector2 Raycast(Vector2 ray)
        {
            if (obstacles.IsEmpty) return default;
            Obstacle obstacle = obstacles[0];
            return ProjectFromOrigin(obstacle.Left, obstacle.Right, ray) ?? throw new Exception();
        }

        public UpdateReport AddObstacle(Obstacle insert)
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

        public UpdateReport RemoveObstacle(Obstacle obstacle)
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


        public struct Obstacle : IEquatable<Obstacle>
        {
            public Vector2 Left;
            public Vector2 Right;

            public bool Equals(Obstacle other) => Left.Equals(other.Left) && Right.Equals(other.Right);
            public override bool Equals(object obj) => obj is Obstacle other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(Left, Right);
        }


        private readonly struct CompareObstacleDistance : IComparer<Obstacle>
        {
            public int Compare(Obstacle x, Obstacle y)
            {
                return SolveFor(x, y) ?? -SolveFor(y, x) ?? 0;

                static int? SolveFor(Obstacle main, Obstacle other)
                {
                    RelativeDomain leftDomain = GetDomain(main.Left, main.Right, other.Left);
                    RelativeDomain rightDomain = GetDomain(main.Left, main.Right, other.Right);
                    RelativeDomain yDomain = Combine(leftDomain, rightDomain);
                    RelativeDomain originDomain = GetOriginDomain(main.Left, main.Right);
                    return yDomain switch
                    {
                        RelativeDomain.Both => null,
                        RelativeDomain.Line => 0,
                        _ => yDomain == originDomain ? 1 : -1,
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