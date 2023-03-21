using System;
using System.Linq;
using Backstreets.Data;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DefaultNamespace
{
    public class PocketPrefabDetails : MonoBehaviour
    {
        [SerializeField] private string _pocketID;
        [SerializeField] private PortalData[] _portals = Array.Empty<PortalData>();
        [SerializeField] private Bounds pocketBounds = default;

        public string PocketID => _pocketID;

        public PortalData[] Portals => _portals;

        public Bounds PocketBounds => pocketBounds;

        public Rect PocketRect => new(pocketBounds.min, pocketBounds.size);

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