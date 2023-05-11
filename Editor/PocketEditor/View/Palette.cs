using UnityEngine;

namespace Backstreets.Editor.PocketEditor.View
{
    public struct Palette
    {
        public Color EdgeColor;
        public Color PortalColor;
        public Color BoundsColor;
        public Color HotColor;

        public Color Get(Color baseColor, bool isHot) =>
            isHot ? HotColor :
            baseColor;

        public Color GetBaseColor(GeometryType type) => type switch
        {
            GeometryType.Edge => EdgeColor,
            GeometryType.Portal => PortalColor,
            GeometryType.Bounds => BoundsColor,
            _ => Color.magenta,
        };

        public static readonly Palette Default = new()
        {
            EdgeColor = Color.white,
            PortalColor = Color.blue,
            BoundsColor = Color.red,
            HotColor = Color.yellow,
        };
    }
}
