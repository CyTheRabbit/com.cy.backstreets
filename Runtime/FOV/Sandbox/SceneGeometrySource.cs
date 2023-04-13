using System.Collections.Generic;
using System.Linq;
using Backstreets.Data;
using Backstreets.FOV.Jobs;
using Backstreets.Pocket;
using UnityEngine.SceneManagement;

namespace Backstreets.FOV.Sandbox
{
    internal class SceneGeometrySource : IGeometrySource
    {
        private Scene scene;

        public SceneGeometrySource(Scene scene)
        {
            this.scene = scene;
        }

        public JobPromise<PocketGeometry> GetGeometry(PocketID pocketID)
        {
            PocketPrefabDetails pocket = GetScenePockets().SingleOrDefault(p => p.PocketID == pocketID);
            if (pocket == null) return JobPromise<PocketGeometry>.Complete(new PocketGeometry(length: 0));

            PocketGeometry result = new(pocket.Edges.Length);
            for (int i = 0; i < pocket.Edges.Length; i++)
            {
                result.Edges[i] = pocket.Edges[i].Line;
            }

            return JobPromise<PocketGeometry>.Complete(result);
        }

        private IEnumerable<PocketPrefabDetails> GetScenePockets() =>
            scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<PocketPrefabDetails>());
    }
}