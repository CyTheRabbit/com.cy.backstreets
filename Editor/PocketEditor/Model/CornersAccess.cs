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
        private readonly GeometryModel model;


        public CornersAccess(PocketPrefabDetails pocket, GeometryModel model)
        {
            this.pocket = pocket;
            this.model = model;
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

        public void Delete(VertexID vertex)
        {
            Assert.IsTrue(pocket.Polygon.IsValidID(vertex));

            Contour contour = pocket.Polygon.contours[vertex.contourIndex];
            contour.Vertices.RemoveAt(vertex.vertexIndex);

            model.RemapEdgeIDs(id =>
                id.contourIndex == vertex.contourIndex && id.edgeIndex >= vertex.vertexIndex
                    ? new EdgeID(id.contourIndex, id.edgeIndex - 1)
                    : id);
        }

        public VertexID Insert(EdgeID splitEdge, float2 position)
        {
            int contourIndex = splitEdge.contourIndex;
            int insertIndex = splitEdge.edgeIndex + 1;

            {
                Contour contour = pocket.Polygon.contours[contourIndex];
                contour.Vertices.Insert(insertIndex, position);
            }

            model.RemapEdgeIDs(id =>
                id.contourIndex == contourIndex && id.edgeIndex >= insertIndex
                    ? new EdgeID(id.contourIndex, id.edgeIndex + 1)
                    : id);

            return new VertexID(contourIndex, insertIndex);
        }
    }
}
