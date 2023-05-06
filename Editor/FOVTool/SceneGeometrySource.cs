using System.Collections.Generic;
using System.Linq;
using Backstreets.Data;
using Backstreets.FOV;
using Backstreets.FOV.Builder;
using Backstreets.FOV.Geometry;
using Backstreets.Pocket;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Backstreets.Editor.FOVTool
{
    internal class SceneGeometrySource : IGeometrySource
    {
        private PocketPrefabDetails[] scenePockets;
        private readonly IEnumerable<PocketPrefabDetails> scenePocketsSource;
        private float lastFetchTime;

        public SceneGeometrySource(Scene scene)
        {
            scenePocketsSource = FetchScenePockets(scene);
        }

        private PocketPrefabDetails[] ScenePockets
        {
            get
            {
                if (Time.realtimeSinceStartup < lastFetchTime + FetchLifetime) return scenePockets;

                lastFetchTime = Time.realtimeSinceStartup;
                return scenePockets = scenePocketsSource.ToArray();
            }
        }

        public PocketGeometry GetGeometry(PocketID pocketID)
        {
            PocketPrefabDetails pocket = FindPocket(pocketID);
            return pocket == null ? PocketGeometry.Nothing(pocketID) : pocket.RuntimeGeometry;
        }

        private PocketPrefabDetails FindPocket(PocketID pocketID)
        {
            foreach (PocketPrefabDetails pocket in ScenePockets)
            {
                if (pocket.PocketID == pocketID) return pocket;
            }

            return null;
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