using System.Collections.Generic;
using System.Linq;
using Backstreets.Data;
using Backstreets.FOV.Builder;
using Backstreets.FOV.Geometry;
using Backstreets.Pocket;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Backstreets.Editor.FOVTool
{
    internal class SceneGeometrySource : IGeometrySource
    {
        private PocketPrefabDetails[] scenePockets;
        private NativeParallelHashMap<PocketID, Color> debugPalette;
        private readonly IEnumerable<PocketPrefabDetails> scenePocketsSource;
        private float lastFetchTime;

        public SceneGeometrySource(Scene scene)
        {
            scenePocketsSource = FetchScenePockets(scene);

            debugPalette = new NativeParallelHashMap<PocketID, Color>(capacity: 64, Allocator.Persistent);
        }

        private PocketPrefabDetails[] ScenePockets
        {
            get
            {
                FetchIfNeeded();
                return scenePockets;
            }
        }

        public NativeParallelHashMap<PocketID, Color> DebugPalette
        {
            get
            {
                FetchIfNeeded();
                return debugPalette;
            }
        }

        public PocketGeometry GetGeometry(PocketID pocketID)
        {
            PocketPrefabDetails pocket = FindPocket(pocketID);
            return pocket == null ? PocketGeometry.Nothing(pocketID) : pocket.RuntimeGeometry;
        }

        public void Dispose()
        {
            debugPalette.Dispose();
        }

        private PocketPrefabDetails FindPocket(PocketID pocketID)
        {
            foreach (PocketPrefabDetails pocket in ScenePockets)
            {
                if (pocket.PocketID == pocketID) return pocket;
            }

            return null;
        }

        private void FetchIfNeeded()
        {
            if (Time.realtimeSinceStartup < lastFetchTime + FetchLifetime) return;
            lastFetchTime = Time.realtimeSinceStartup;

            {
                scenePockets = scenePocketsSource.ToArray();
            }

            {
                debugPalette.Clear();
                foreach (PocketPrefabDetails pocket in scenePockets)
                {
                    debugPalette.Add(pocket.PocketID, pocket.DebugColor);
                }
            }
        }


        private const float FetchLifetime = 1f;

        private static IEnumerable<PocketPrefabDetails> FetchScenePockets(Scene scene)
        {
            foreach (GameObject go in scene.GetRootGameObjects())
            foreach (PocketPrefabDetails pocket in go.GetComponentsInChildren<PocketPrefabDetails>())
            {
                yield return pocket;
            }
        }
    }
}