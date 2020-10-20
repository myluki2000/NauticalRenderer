using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace NauticalRenderer.SlippyMap.Data
{
    struct Landmark
    {
        public LandmarkCategory Category { get; }
        public Vector2 Coordinates { get; }

        public Landmark(LandmarkCategory category, Vector2 coordinates)
        {
            Category = category;
            Coordinates = coordinates;
        }

        public enum LandmarkCategory
        {
            CAIRN,
            CEMETERY,
            CHIMNEY,
            DISH_AERIAL,
            FLAGSTAFF,
            FLARE_STACK,
            MAST,
            WINDSOCK,
            MONUMENT,
            COLUMN,
            MEMORIAL,
            OBELISK,
            STATUE,
            CROSS,
            DOME,
            RADAR_SCANNER,
            TOWER,
            WINDMILL,
            WINDMOTOR,
            SPIRE,
            BOULDER,
            GENERIC
        }
    }
}
