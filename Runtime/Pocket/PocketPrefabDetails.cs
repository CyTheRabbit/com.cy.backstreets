using System;
using System.Linq;
using Backstreets.Data;
using Backstreets.FOV.Geometry;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Backstreets.Pocket
{
    public class PocketPrefabDetails : MonoBehaviour
    {
        [SerializeField] private int pocketID;
        [SerializeField] private Color debugColor;
        [SerializeField] private EdgeData[] edges = Array.Empty<EdgeData>();
        [SerializeField] private PortalData[] portals = Array.Empty<PortalData>();
        [SerializeField] private Bounds pocketBounds = default;

        private PocketGeometry? runtimeGeometry;

        public PocketGeometry RuntimeGeometry => runtimeGeometry ??= MakeRuntimeGeometry(Allocator.Persistent);

        public PocketID PocketID => new(pocketID);

        public Color DebugColor => debugColor;

        public PortalData[] Portals
        {
            get => portals;
            set => portals = value;
        }

        public Bounds PocketBounds => pocketBounds;

        public Rect PocketRect => new(pocketBounds.min, pocketBounds.size);

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

        private Bounds CalculatePocketBounds()
        {
            Tilemap[] tilemaps = GetComponentsInChildren<Tilemap>();
            if (!tilemaps.Any()) return default;

            Bounds cumulativeBounds = GetCompressedBounds(tilemaps.First());
            foreach (Tilemap tilemap in tilemaps.Skip(1))
            {
                cumulativeBounds.Encapsulate(GetCompressedBounds(tilemap));
            }

            return tilemaps
                .Select(GetCompressedBounds)
                .Aggregate(
                    seed: (Bounds?) null,
                    func: (accumulator, current) =>
                    {
                        if (accumulator is not { } previous) return current;
                        previous.Encapsulate(current);
                        return previous;
                    },
                    resultSelector: accumulator => accumulator ?? new Bounds());

            static Bounds GetCompressedBounds(Tilemap tilemap)
            {
                tilemap.CompressBounds();
                Bounds localBounds = tilemap.localBounds;
                Matrix4x4 localToWorld = tilemap.transform.localToWorldMatrix;
                Bounds worldBounds = new(localToWorld.MultiplyPoint3x4(localBounds.center), Vector3.zero);
                for (int x = 0; x < 2; x++)
                for (int y = 0; y < 2; y++)
                for (int z = 0; z < 2; z++)
                {
                    Vector3 corner = new(
                        x == 0 ? localBounds.min.x : localBounds.max.x,
                        y == 0 ? localBounds.min.y : localBounds.max.y,
                        z == 0 ? localBounds.min.z : localBounds.max.z);
                    worldBounds.Encapsulate(localToWorld.MultiplyPoint3x4(corner));
                }

                return worldBounds;
            }
        }
    }
}
