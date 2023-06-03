using System;
using System.Collections.Generic;
using Backstreets.FOV.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Backstreets.Data
{
    [Serializable]
    public class Contour
    {
        [SerializeField] private List<float2> vertices = new();


        public List<float2> Vertices => vertices;

        public EdgeList Edges => new(vertices);


        public Orientation GetOrientation()
        {
            if (vertices.Count < 3) return Orientation.Unknown;

            float determinantSum = 0;
            foreach (Line edge in Edges)
            {
                determinantSum += LineMath.Determinant(edge.Right, edge.Left);
            }

            return determinantSum switch
            {
                < 0 => Orientation.CW,
                > 0 => Orientation.CCW,
                _ => Orientation.Unknown
            };
        }


        public enum Orientation
        {
            Unknown,
            CW,
            CCW,
        }

        public readonly struct EdgeList
        {
            private readonly List<float2> vertices;

            public EdgeList(List<float2> vertices) => this.vertices = vertices;

            public int Count => vertices.Count < 3 ? 0 : vertices.Count;

            public Line this[int index]
            {
                get => index + 1 == vertices.Count
                    ? new Line(vertices[index], vertices[0])
                    : new Line(vertices[index], vertices[index + 1]);
                set => Set(index, value);
            }


            public void Set(int index, Line value)
            {
                int nextIndex = index + 1 == vertices.Count ? 0 : index + 1;
                vertices[index] = value.Right;
                vertices[nextIndex] = value.Left;
            }

            public EdgeEnumerator GetEnumerator() => new(this);
        }

        public struct EdgeEnumerator
        {
            private readonly EdgeList edges;
            private int index;

            public EdgeEnumerator(EdgeList edges)
            {
                this.edges = edges;
                index = -1;
            }

            public Line Current => edges[index];

            public bool MoveNext() => ++index < edges.Count;
        }
    }
}
