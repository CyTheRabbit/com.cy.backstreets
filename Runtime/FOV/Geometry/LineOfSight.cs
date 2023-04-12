using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Backstreets.FOV.Geometry.LineMath;

namespace Backstreets.FOV.Geometry
{
    [BurstCompatible]
    internal struct LineOfSight : INativeDisposable
    {
        private NativeList<Line> obstacles; // It might be more efficient to use a linked list.
        private NativeList<int> obstacleIds;
        private float2 ray;

        public LineOfSight(int capacity)
        {
            obstacles = new NativeList<Line>(capacity, Allocator.TempJob);
            obstacleIds = new NativeList<int>(capacity, Allocator.TempJob);
            ray = default;
        }

        public readonly float2 Raycast() =>
            obstacles.IsEmpty ? default : ProjectFromOrigin(obstacles[0], ray) ?? throw new Exception();

        public readonly int RaycastId() => 
            obstacleIds.IsEmpty ? InvalidEdgeID : obstacleIds[0];

        public void LookAt(float2 direction) => ray = direction;

        public void LookAt(Corner corner) => ray = math.normalizesafe(corner.Position);

        public UpdateReport Update(Corner corner)
        {
            bool isPerpendicularToOrigin = GetOriginDomain(corner.Line) is LineDomain.Line;
            if (isPerpendicularToOrigin) return default;

            return corner.End switch
            {
                Corner.Endpoint.Right => AddObstacle(corner.Line, corner.LineIndex),
                Corner.Endpoint.Left => RemoveObstacle(corner.LineIndex),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public UpdateReport AddObstacle(Line insert, int id)
        {
            CompareObstacleDistance comparer = new();
            int insertIndex;
            for (insertIndex = 0; insertIndex < obstacles.Length; insertIndex++)
            {
                if (comparer.Compare(insert, obstacles[insertIndex]) < 0) break;
            }

            if (insertIndex == obstacles.Length)
            {
                AppendObstacle(insert, id);
            }
            else
            {
                InsertObstacle(insert, id, insertIndex);
            }

            return new UpdateReport
            {
                ClosestObstacleChanged = insertIndex == 0,
            };
        }

        public UpdateReport RemoveObstacle(int id)
        {
            int index = obstacleIds.IndexOf(id);
            if (index < 0) return new UpdateReport { OperationFailed = true };

            RemoveObstacleAtIndex(index);
            return new UpdateReport
            {
                ClosestObstacleChanged = index == 0
            };
        }

        public void Dispose()
        {
            obstacles.Dispose();
            obstacleIds.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps) => JobHandle.CombineDependencies(
            obstacles.Dispose(inputDeps),
            obstacleIds.Dispose(inputDeps));

        private void AppendObstacle(Line insert, int id)
        {
            obstacles.Add(insert);
            obstacleIds.Add(id);
        }

        private void InsertObstacle(Line insert, int id, int insertIndex)
        {
            obstacles.InsertRangeWithBeginEnd(insertIndex, insertIndex + 1);
            obstacles[insertIndex] = insert;
            obstacleIds.InsertRangeWithBeginEnd(insertIndex, insertIndex + 1);
            obstacleIds[insertIndex] = id;
        }

        private void RemoveObstacleAtIndex(int index)
        {
            obstacles.RemoveAt(index);
            obstacleIds.RemoveAt(index);
        }


        private const int InvalidEdgeID = -1;


        private readonly struct CompareObstacleDistance : IComparer<Line>
        {
            public int Compare(Line x, Line y)
            {
                return SolveFor(x, y) ?? -SolveFor(y, x) ?? 0;

                static int? SolveFor(Line main, Line other)
                {
                    LineDomain rightDomain = GetDomain(main, other.Right);
                    LineDomain leftDomain = GetDomain(main, other.Left);
                    LineDomain otherDomain = Combine(rightDomain, leftDomain);
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
            public bool OperationFailed;


            public static UpdateReport operator +(UpdateReport x, UpdateReport y) =>
                new()
                {
                    ClosestObstacleChanged = x.ClosestObstacleChanged || y.ClosestObstacleChanged,
                    OperationFailed = x.OperationFailed || y.OperationFailed,
                };
        }
    }
}