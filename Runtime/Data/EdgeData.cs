using System;
using Backstreets.FOV.Geometry;
using Unity.Mathematics;

namespace Backstreets.Data
{
    [Serializable]
    public struct EdgeData
    {
        public int id;
        public float2 right;
        public float2 left;

        public Line Line
        {
            get => new(right, left);
            set => (right, left) = (value.Right, value.Left);
        }
    }
}