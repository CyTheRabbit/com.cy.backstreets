using System;
using System.Collections.Generic;
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
        [SerializeField] private Polygon polygon;
        [SerializeField] private List<PortalData> portals = new();
        [SerializeField] private Bounds pocketBounds;

        private PocketGeometry? runtimeGeometry;

        public PocketGeometry RuntimeGeometry => runtimeGeometry ??= MakeRuntimeGeometry(Allocator.Persistent);

        public PocketID PocketID => new(pocketID);

        public Color DebugColor => debugColor;

        public Polygon Polygon
        {
            get => polygon ??= new Polygon();
            set => polygon = value;
        }

        public List<PortalData> Portals
        {
            get => portals;
            set => portals = value;
        }

        public Rect PocketRect => new(pocketBounds.min, pocketBounds.size);


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
            if (polygon is null)
            {
                return PocketGeometry.Nothing(id);
            }

            int edgeCount = polygon.EdgeCount;
            PocketGeometry geometry = new(id, edgeCount, portals.Count, allocator);
            using NativeParallelHashMap<EdgeID, int> edgeIdToIndex = new(edgeCount, Allocator.Temp);

            {
                int index = 0;
                foreach ((EdgeID edgeID, Line edge) in polygon.EnumerateEdgesWithIDs())
                {
                    edgeIdToIndex.Add(edgeID, index);
                    geometry.Edges[index++] = edge;
                }
            }

            for (int i = 0; i < portals.Count; i++)
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
