using System;
using Backstreets.Data;
using Backstreets.FOV.Geometry;
using Unity.Mathematics;

namespace Backstreets.Editor.PocketEditor.Model
{
    internal struct CornerData
    {
        public CornerData(int edgeID, Endpoint end, float2 position)
        {
            EdgeID = edgeID;
            End = end;
            Position = position;
        }

        public CornerData(EdgeData edge, Endpoint end)
        {
            EdgeID = edge.id;
            End = end;
            Position = end switch
            {
                Endpoint.Right => edge.right,
                Endpoint.Left => edge.left,
                _ => throw new ArgumentOutOfRangeException(nameof(end), end, null)
            };
        }

        public int EdgeID;
        public Endpoint End;
        public float2 Position;

        internal enum Endpoint
        {
            Right,
            Left,
        }
    }
}
