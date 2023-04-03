using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Backstreets.FOV.Sandbox
{
    public class ViewportObstacle : MonoBehaviour
    {
        [SerializeField] private float2[] vertices = Array.Empty<float2>();


        public float2[] Vertices => vertices;


        private void OnDrawGizmos()
        {
            if (vertices.Length < 2) return;

            Handles.color = Color.white;
            IEnumerable<float2> shiftedVertices = vertices.Skip(1).Append(vertices.First());
            IEnumerable<(float2, float2)> lines = vertices.Zip(shiftedVertices,
                (first, second) => (first, second));

            foreach ((Vector2 first, Vector2 second) in lines)
            {
                Handles.DrawLine(first, second, 2);
            }
        }
    }
}