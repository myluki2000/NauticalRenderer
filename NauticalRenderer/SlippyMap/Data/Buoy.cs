using System;
using System.Collections.Generic;
using System.Text;

namespace NauticalRenderer.SlippyMap.Data
{
    struct Buoy
    {
        public BuoyType Type { get; set; }
        public BuoyShape Shape { get; set; }

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
    }
}
