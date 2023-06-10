using System;
using System.Linq;
using Backstreets.Data;
using Backstreets.FOV.Geometry;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Backstreets.Pocket
{
    public class PocketPrefabDetails : MonoBehaviour
    {
        [SerializeField] private int pocketID;
        [SerializeField] private Color debugColor;
        [SerializeField] private EdgeData[] edges = Array.Empty<EdgeData>();
        [SerializeField] private PortalData[] portals = Array.Empty<PortalData>();
        [SerializeField] private Rect pocketBounds;

        private PocketGeometry? runtimeGeometry;

        public PocketGeometry RuntimeGeometry => runtimeGeometry ??= MakeRuntimeGeometry(Allocator.Persistent);

        public PocketID PocketID => new(pocketID);

        public Color DebugColor => debugColor;

        public PortalData[] Portals
        {
            get => portals;
            set => portals = value;
        }

        public Rect PocketRect => pocketBounds;

        public EdgeData[] Edges
        {
            get => edges;
            set => edges = value;
        }

        public Line? FindEdge(int id)
        {
            int index = Array.FindIndex(edges, edge => edge.id == id);
            return index == -1 ? null : edges[index].Line;
        }

        public void OnValidate()
        {
            pocketBounds = CalculatePocketBounds();

            runtimeGeometry?.Dispose();
            runtimeGeometry = null;
        }

        public void OnDestroy()
        {
            runtimeGeometry?.Dispose();
            runtimeGeometry = null;
        }

        private PocketGeometry MakeRuntimeGeometry(Allocator allocator)
        {
            PocketID id = new(pocketID);
            PocketGeometry geometry = new(id, edges.Length, portals.Length, allocator);
            using NativeParallelHashMap<int, int> edgeIdToIndex = new(edges.Length, Allocator.Temp);

            for (int i = 0; i < edges.Length; i++)
            {
                EdgeData edgeData = edges[i];
                edgeIdToIndex.Add(edgeData.id, i);
                geometry.Edges[i] = edgeData.Line;
            }

            for (int i = 0; i < portals.Length; i++)
            {
                PortalData portalData = portals[i];
                int edgeIndex = edgeIdToIndex[portalData.edgeID];
                Line edge = geometry.Edges[edgeIndex];
                PocketID exit = new(portalData.exitID);
                geometry.Portals[i] = new Portal(edge, edgeIndex, entrance: id, exit);
            }

            return geometry;
        }

        private Rect CalculatePocketBounds()
        {
            return edges is { Length: > 0 }
                ? edges.Aggregate(seed: GetAABB(edges[0]), EncapsulateEdge)
                : Rect.zero;

            static Rect EncapsulateEdge(Rect rect, EdgeData edge) =>
                EncapsulatePoint(EncapsulatePoint(rect, edge.right), edge.left);

            static Rect EncapsulatePoint(Rect rect, float2 point) =>
                Rect.MinMaxRect(
                    xmin: math.min(rect.xMin, point.x),
                    ymin: math.min(rect.yMin, point.y),
                    xmax: math.max(rect.xMax, point.x),
                    ymax: math.max(rect.yMax, point.y));

            static Rect GetAABB(EdgeData edge) =>
                Rect.MinMaxRect(
                    xmin: math.min(edge.right.x, edge.left.x),
                    ymin: math.min(edge.right.y, edge.left.y),
                    xmax: math.max(edge.right.x, edge.left.x),
                    ymax: math.max(edge.right.x, edge.left.x));
        }
    }
}
