using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NauticalRenderer.SlippyMap.Data
{
    public static class MapStyle
    {
        public static readonly Color COLOR_WATER = Color.FromNonPremultiplied(58, 147, 214, 255);
        public static readonly Color COLOR_INLAND_WATER = Color.FromNonPremultiplied(113, 177, 225, 255);
        public static readonly Color COLOR_LAND = Color.FromNonPremultiplied(210, 200, 175 , 255);
        public static readonly Color COLOR_LAND_OUTLINE = Color.FromNonPremultiplied(60, 45, 13, 255);
        public static readonly Color COLOR_TIDAL_FLATS = Color.FromNonPremultiplied(112, 148, 174, 255);
        public static readonly Color COLOR_TIDAL_FLATS_OUTLINE = Color.FromNonPremultiplied(80, 122, 150, 255);
        public static readonly Color COLOR_FOREST = Color.FromNonPremultiplied(110, 150, 80, 255);
        public static readonly Color COLOR_BUILDINGS = Color.FromNonPremultiplied(220, 115, 135, 255);
    }
}
