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
        private NativeList<Line> edges; // It might be more efficient to use a linked list.
        private NativeList<int> edgeIds;
        private float2 ray;

        public LineOfSight(int capacity)
        {
            edges = new NativeList<Line>(capacity, Allocator.TempJob);
            edgeIds = new NativeList<int>(capacity, Allocator.TempJob);
            ray = default;
        }

        public readonly float2 Raycast() =>
            edges.IsEmpty ? default : ProjectFromOrigin(edges[0], ray) ?? throw new Exception();

        public readonly int RaycastId() => 
            edgeIds.IsEmpty ? InvalidEdgeID : edgeIds[0];

        public void LookAt(float2 direction) => ray = direction;

        public void LookAt(Corner corner) => ray = math.normalizesafe(corner.Position);

        public UpdateReport Update(Corner corner)
        {
            bool isPerpendicularToOrigin = GetOriginDomain(corner.Edge) is LineDomain.Line;
            if (isPerpendicularToOrigin) return default;

            return corner.End switch
            {
                Corner.Endpoint.Right => AddEdge(corner.Edge, corner.EdgeIndex),
                Corner.Endpoint.Left => RemoveEdge(corner.EdgeIndex),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public UpdateReport AddEdge(Line insert, int id)
        {
            CompareEdgeDistance comparer = new();
            int insertIndex;
            for (insertIndex = 0; insertIndex < edges.Length; insertIndex++)
            {
                if (comparer.Compare(insert, edges[insertIndex]) < 0) break;
            }

            if (insertIndex == edges.Length)
            {
                AppendEdge(insert, id);
            }
            else
            {
                InsertEdge(insert, id, insertIndex);
            }

            return new UpdateReport
            {
                ClosestEdgeChanged = insertIndex == 0,
            };
        }

        public UpdateReport RemoveEdge(int id)
        {
            int index = edgeIds.IndexOf(id);
            if (index < 0) return new UpdateReport { OperationFailed = true };

            RemoveEdgeAtIndex(index);
            return new UpdateReport
            {
                ClosestEdgeChanged = index == 0
            };
        }

        public void Dispose()
        {
            edges.Dispose();
            edgeIds.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps) => JobHandle.CombineDependencies(
            edges.Dispose(inputDeps),
            edgeIds.Dispose(inputDeps));

        private void AppendEdge(Line insert, int id)
        {
            edges.Add(insert);
            edgeIds.Add(id);
        }

        private void InsertEdge(Line insert, int id, int insertIndex)
        {
            edges.InsertRangeWithBeginEnd(insertIndex, insertIndex + 1);
            edges[insertIndex] = insert;
            edgeIds.InsertRangeWithBeginEnd(insertIndex, insertIndex + 1);
            edgeIds[insertIndex] = id;
        }

        private void RemoveEdgeAtIndex(int index)
        {
            edges.RemoveAt(index);
            edgeIds.RemoveAt(index);
        }


        private const int InvalidEdgeID = -1;


        private readonly struct CompareEdgeDistance : IComparer<Line>
        {
            public int Compare(Line x, Line y)
            {
                // Shrink lines insignificantly in case they have a shared corner.
                Line xShrunk = Shrink(x);
                Line yShrunk = Shrink(y);

                return SolveFor(xShrunk, yShrunk) ?? -SolveFor(yShrunk, xShrunk) ?? 0;

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

            private static Line Shrink(Line line) => new(
                right: math.lerp(line.Right, line.Left, CutoffRatio),
                left: math.lerp(line.Right, line.Left, 1 - CutoffRatio));

            private const float CutoffRatio = 0.01f;
        }

        public struct UpdateReport
        {
            public bool ClosestEdgeChanged;
            public bool OperationFailed;


            public static UpdateReport operator +(UpdateReport x, UpdateReport y) =>
                new()
                {
                    ClosestEdgeChanged = x.ClosestEdgeChanged || y.ClosestEdgeChanged,
                    OperationFailed = x.OperationFailed || y.OperationFailed,
                };
        }
    }
}