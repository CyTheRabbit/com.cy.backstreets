using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.View
{
    public struct Palette
    {
        public Color EdgeColor;
        public Color PortalColor;
        public Color CornerColor;
        public Color BoundsColor;

        public Color Get(Color baseColor, bool isHot) =>
            isHot ? Handles.selectedColor :
            baseColor;

        public Color GetBaseColor(GeometryType type) => type switch
        {
            GeometryType.Edge => EdgeColor,
            GeometryType.Portal => PortalColor,
            GeometryType.Bounds => BoundsColor,
            GeometryType.Corner => CornerColor,
            _ => Color.magenta,
        };

        public static readonly Palette Default = new()
        {
            EdgeColor = Color.white,
            PortalColor = Color.blue,
            CornerColor = Color.white,
            BoundsColor = Color.red,
        };
    }
}
