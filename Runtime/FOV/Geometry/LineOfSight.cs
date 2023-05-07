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

        public LineOfSight(int capacity, Allocator allocator)
        {
            edges = new NativeList<Line>(capacity, allocator);
            edgeIds = new NativeList<int>(capacity, allocator);
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
            public int Compare(Line x, Line y) => CompareLineDistance(x, y);
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