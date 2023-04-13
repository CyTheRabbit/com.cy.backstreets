using System;
using System.Linq;
using Backstreets.Data;
using Backstreets.FOV.Geometry;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Backstreets.Pocket
{
    public class PocketPrefabDetails : MonoBehaviour
    {
        [SerializeField] private int pocketID;
        [SerializeField] private EdgeData[] edges;
        [SerializeField] private PortalData[] portals = Array.Empty<PortalData>();
        [SerializeField] private Bounds pocketBounds = default;

        public PocketID PocketID => new(pocketID);

        public PortalData[] Portals => portals;

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

        private void OnValidate()
        {
            pocketBounds = CalculatePocketBounds();
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