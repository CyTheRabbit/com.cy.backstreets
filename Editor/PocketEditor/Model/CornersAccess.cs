using System.Collections.Generic;
using Backstreets.Data;
using Backstreets.Pocket;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Backstreets.Editor.PocketEditor.Model
{
    internal class CornersAccess
    {
        private readonly PocketPrefabDetails pocket;


        public CornersAccess(PocketPrefabDetails pocket)
        {
            this.pocket = pocket;
        }

        public IEnumerable<(VertexID ID, float2 Position)> All => pocket.Polygon.EnumerateVerticesWithIDs();

        public IEnumerable<VertexID> AllIDs => pocket.Polygon.EnumerateVertexIDs();


        public float2 Get(VertexID id)
        {
            Assert.IsTrue(pocket.Polygon.IsValidID(id));
            return pocket.Polygon[id];
        }

        public void Update(VertexID id, float2 data)
        {
            Assert.IsTrue(pocket.Polygon.IsValidID(id));
            pocket.Polygon[id] = data;
        }

        public void Delete(VertexID id)
        {
            Assert.IsTrue(pocket.Polygon.IsValidID(id));

            Contour contour = pocket.Polygon.contours[id.contourIndex];
            contour.Vertices.RemoveAt(id.vertexIndex);

            EdgeID updatedEdge = new(id.contourIndex, id.vertexIndex);
            for (int i = 0; i < pocket.Portals.Count; i++)
            {
                PortalData portal = pocket.Portals[i];
                if (!IsAffectedByIndexUpdate(portal.edgeID, updatedEdge)) continue;

                portal.edgeID.edgeIndex--;
                pocket.Portals[i] = portal;
            }
        }

        public VertexID Insert(EdgeID splitEdge, float2 position)
        {
            int contourIndex = splitEdge.contourIndex;
            int insertIndex = splitEdge.edgeIndex + 1;

            {
                Contour contour = pocket.Polygon.contours[contourIndex];
                contour.Vertices.Insert(insertIndex, position);
            }

            for (int i = 0; i < pocket.Portals.Count; i++)
            {
                PortalData portal = pocket.Portals[i];
                if (!IsAffectedByIndexUpdate(portal.edgeID, splitEdge)) continue;

                portal.edgeID.edgeIndex++;
                pocket.Portals[i] = portal;
            }

            return new VertexID(contourIndex, insertIndex);
        }

        private static bool IsAffectedByIndexUpdate(EdgeID edge, EdgeID updated) =>
            edge.contourIndex == updated.contourIndex &&
            edge.edgeIndex >= updated.edgeIndex;
    }
}
