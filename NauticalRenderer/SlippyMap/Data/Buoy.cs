using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace NauticalRenderer.SlippyMap.Data
{
    struct Buoy
    {
        public Vector2 Coordinates { get; }
        public BuoyType Type { get; }
        public BuoyShape Shape { get; }
        public BuoyColorPattern ColorPattern { get; set; }
        public Color[] Colors { get; }

        public Buoy(Vector2 coordinates, BuoyType type, BuoyShape shape, BuoyColorPattern colorPattern, Color[] colors)
        {
            Coordinates = coordinates;
            Type = type;
            Shape = shape;
            ColorPattern = colorPattern;
            Colors = colors;
        }

        public enum BuoyType
        {
            BUOY_CARDINAL,
            BUOY_INSTALLATION,
            BUOY_ISOLATED_DANGER,
            BUOY_LATERAL,
            MOORING,
            BUOY_SAFE_WATER,
            BUOY_SPECIAL_PURPOSE
        }

        public enum BuoyShape
        {
            CONICAL,
            CAN,
            SPHERICAL,
            PILLAR,
            SPAR,
            BARREL,
            SUPER_BUOY,
            ICE_BUOY
        }

        public enum BuoyColorPattern
        {
            NONE,
            HORIZONTAL,
            VERTICAL,
            DIAGONAL,
            SQUARED,
            STRIPES,
            BORDER,
            CROSS,
            SALTIRE,
        }
    }
}
