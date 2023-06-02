using System;
using System.Collections.Generic;
using System.Linq;
using Backstreets.FOV.Geometry;
using Unity.Mathematics;

namespace Backstreets.Data
{
    [Serializable]
    public class Polygon
    {
        public List<Contour> contours = new();


        public bool IsValid => contours is not null;

        public int EdgeCount => contours.Sum(c => c.Edges.Count);

        public int HoleCount => contours.Count - 1;

        public float2 this[VertexID id]
        {
            get => contours[id.contourIndex].Vertices[id.vertexIndex];
            set => contours[id.contourIndex].Vertices[id.vertexIndex] = value;
        }

        public Line this[EdgeID id]
        {
            get => contours[id.contourIndex].Edges[id.edgeIndex];
            set => contours[id.contourIndex].Edges.Set(id.edgeIndex, value);
        }


        public bool IsValidID(VertexID id) =>
            id.contourIndex < contours.Count &&
            id.vertexIndex < contours[id.contourIndex].Vertices.Count;

        public bool IsValidID(EdgeID id) =>
            id.contourIndex < contours.Count &&
            id.edgeIndex < contours[id.contourIndex].Edges.Count;

        public bool TryGet(EdgeID id, out Line edge)
        {
            if (id.contourIndex < contours.Count)
            {
                Contour.EdgeList contour = contours[id.contourIndex].Edges;
                if (id.edgeIndex < contour.Count)
                {
                    edge = contour[id.edgeIndex];
                    return true;
                }
            }

            edge = default;
            return false;
        }

        public bool TryGet(VertexID id, out float2 vertex)
        {
            if (id.contourIndex < contours.Count)
            {
                List<float2> vertices = contours[id.vertexIndex].Vertices;
                if (id.vertexIndex < vertices.Count)
                {
                    vertex = vertices[id.vertexIndex];
                    return true;
                }
            }

            vertex = default;
            return false;
        }

        #region Enumerators

        public IEnumerable<VertexID> EnumerateVertexIDs()
        {
            for (int contourIndex = 0; contourIndex < contours.Count; contourIndex++)
            {
                int vertexCount = contours[contourIndex].Vertices.Count;
                for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
                {
                    yield return new VertexID(contourIndex, vertexIndex);
                }
            }
        }

        public IEnumerable<EdgeID> EnumerateEdgeIDs()
        {
            for (int contourIndex = 0; contourIndex < contours.Count; contourIndex++)
            {
                int edgeCount = contours[contourIndex].Edges.Count;
                for (int edgeIndex = 0; edgeIndex < edgeCount; edgeIndex++)
                {
                    yield return new EdgeID(contourIndex, edgeIndex);
                }
            }
        }

        public IEnumerable<(VertexID, float2)> EnumerateVerticesWithIDs()
        {
            for (int contourIndex = 0; contourIndex < contours.Count; contourIndex++)
            {
                List<float2> vertices = contours[contourIndex].Vertices;
                for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
                {
                    yield return (new VertexID(contourIndex, vertexIndex), vertices[vertexIndex]);
                }
            }
        }

        public IEnumerable<(EdgeID, Line)> EnumerateEdgesWithIDs()
        {
            for (int contourIndex = 0; contourIndex < contours.Count; contourIndex++)
            {
                Contour.EdgeList edges = contours[contourIndex].Edges;
                for (int edgeIndex = 0; edgeIndex < edges.Count; edgeIndex++)
                {
                    yield return (new EdgeID(contourIndex, edgeIndex), edges[edgeIndex]);
                }
            }
        }

        #endregion
    }
}
