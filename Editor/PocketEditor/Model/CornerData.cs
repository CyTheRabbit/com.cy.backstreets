using System;
using Backstreets.Data;
using Unity.Mathematics;
using UnityEngine;

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

        public CornerData Offset(Vector2 offset)
        {
            CornerData copy = this;
            copy.Position += (float2)offset;
            return copy;
        }

        internal enum Endpoint
        {
            Right,
            Left,
        }
    }
}
