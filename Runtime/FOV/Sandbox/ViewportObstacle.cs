using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Backstreets.FOV.Sandbox
{
    public class ViewportObstacle : MonoBehaviour
    {
        [SerializeField] private Vector2[] vertices = Array.Empty<Vector2>();


        public Vector2[] Vertices => vertices;


        private void OnDrawGizmos()
        {
            if (vertices.Length < 2) return;

            Handles.color = Color.white;
            IEnumerable<Vector2> shiftedVertices = vertices.Skip(1).Append(vertices.First());
            IEnumerable<(Vector2, Vector2)> lines = vertices.Zip(shiftedVertices,
                (first, second) => (first, second));

            foreach ((Vector2 first, Vector2 second) in lines)
            {
                Handles.DrawLine(first, second, 2);
            }
        }
    }
}