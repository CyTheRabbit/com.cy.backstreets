using System.Collections.Generic;
using System.Linq;
using Backstreets.Data;
using Backstreets.Editor.PocketEditor.View;
using Backstreets.Pocket;

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

        public IEnumerable<CornerData> All => RightCorners.Concat(LeftCorners);

        public IEnumerable<CornerData> RightCorners =>
            pocket.Edges.Select(edge => new CornerData(edge, CornerData.Endpoint.Right));

        public IEnumerable<CornerData> LeftCorners =>
            pocket.Edges.Select(edge => new CornerData(edge, CornerData.Endpoint.Left));


        public CornerData Get(GeometryID id)
        {
            Validation.AssertGeometryType(id, GeometryType.Corner);
            GeometryID edgeID = new(GeometryType.Edge, id.ID);
            EdgeData edge = Validation.FindItem(pocket.Edges, edgeID, GeometryID.Of);

            return new CornerData(edge, (CornerData.Endpoint)id.Extra);
        }

        public void Update(CornerData corner)
        {
            GeometryID edgeID = new(GeometryType.Edge, corner.EdgeID);
            int edgeIndex = Validation.FindIndex(pocket.Edges, edgeID, GeometryID.Of);

            using (model.RecordChanges($"Update {corner}"))
            {
                switch (corner.End)
                {
                    case CornerData.Endpoint.Right:
                        pocket.Edges[edgeIndex].right = corner.Position;
                        break;
                    case CornerData.Endpoint.Left:
                        pocket.Edges[edgeIndex].left = corner.Position;
                        break;
                }
            }
        }

        public void UpdateBatch(CornerData[] corners)
        {
            int[] edgeIndices = corners
                .Select(corner => Validation.FindIndex(pocket.Edges, new GeometryID(GeometryType.Edge, corner.EdgeID), GeometryID.Of))
                .ToArray(); // Precompute indices to validate them before applying changes

            using (model.RecordChanges($"Update corners ({corners.Length})"))
            {
                for (int i = 0; i < corners.Length; i++)
                {
                    CornerData corner = corners[i];
                    int edgeIndex = edgeIndices[i];

                    switch (corner.End)
                    {
                        case CornerData.Endpoint.Right:
                            pocket.Edges[edgeIndex].right = corner.Position;
                            break;
                        case CornerData.Endpoint.Left:
                            pocket.Edges[edgeIndex].left = corner.Position;
                            break;
                    }
                }
            }
        }
    }
}
