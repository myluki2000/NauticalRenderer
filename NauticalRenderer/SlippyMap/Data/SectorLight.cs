using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace NauticalRenderer.SlippyMap.Data
{
    public struct SectorLight
    {
        public Vector2 Coordinates;
        public bool Major;
        public Sector[] Sectors;

        public struct Sector
        {
            public float StartAngle;
            public float EndAngle;
            public float Orientation;
            public float Range;
            public Color Color;
        }
    }
}
