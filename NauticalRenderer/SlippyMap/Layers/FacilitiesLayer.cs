using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Data;
using NauticalRenderer.Utility;
using OsmSharp.Streams;

namespace NauticalRenderer.SlippyMap.Layers
{
    class FacilitiesLayer : MapLayer
    {
        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        private Facility[] facilities;

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            facilities = new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete()
                .Where(x => x.Tags.ContainsKey("seamark:small_craft_facility:category"))
                .Select(x =>
                {
                    string[] categories = x.Tags["seamark:small_craft_facility:category"].Split(';');
                    Facility.FacilityCategory category = Facility.FacilityCategory.NONE;
                    foreach (string s in categories)
                    {
                        Enum.TryParse(s.Replace('-', '_'), true, out Facility.FacilityCategory nc);
                        category |= nc;
                    }
                    return new Facility(OsmHelpers.GetCoordinateOfOsmGeo(x), category);
                })
                .Where(x => x.Category != Facility.FacilityCategory.NONE)
                .ToArray();
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            if (camera.Scale.Y > 90000)
                foreach (Facility f in facilities)
                {
                    if (!camera.DrawBounds.Contains(f.Coordinates)) continue;

                    Vector2 screenPos = f.Coordinates.Transform(camera.GetMatrix());
                    Facility.FacilityCategory[] cats = EnumUtils.GetFlags(f.Category).ToArray();

                    const int WIDTH = 30;
                    for (int i = 0; i < cats.Length; i++)
                    {
                        sb.Draw(Icons.Buoys,
                            new Rectangle(screenPos.ToPoint() + new Point(i * WIDTH - (cats.Length * WIDTH) / 2, 0), new Point(WIDTH, WIDTH)),
                            new Rectangle(GetAtlasCoordsForCategory(cats[i]), new Point(512, 512)),
                            Color.White);
                    }
                }
        }

        private Point GetAtlasCoordsForCategory(Facility.FacilityCategory category)
        {
            switch (category)
            {
                case Facility.FacilityCategory.SLIPWAY:
                    return new Point(0, 0);
                case Facility.FacilityCategory.VISITOR_BERTH:
                    return new Point(512, 0);
            }

            return category switch
            {
                Facility.FacilityCategory.SLIPWAY => new Point(0, 0),
                Facility.FacilityCategory.VISITOR_BERTH => new Point(512, 0),
                Facility.FacilityCategory.BOAT_HOIST => new Point(1024, 0),
                Facility.FacilityCategory.FUEL_STATION => new Point(1536, 0),
                Facility.FacilityCategory.PUMP_OUT => new Point(0, 512),
                Facility.FacilityCategory.ELECTRICITY => new Point(512, 512),
                Facility.FacilityCategory.BOATYARD => new Point(1024, 512),
                Facility.FacilityCategory.TOILETS => new Point(1536, 512),
                Facility.FacilityCategory.CHANDLER => new Point(0, 1024),
                Facility.FacilityCategory.SHOWERS => new Point(512, 1024),
                Facility.FacilityCategory.NAUTICAL_CLUB => new Point(1024, 1024),
                Facility.FacilityCategory.VISITORS_MOORING => new Point(1536, 1024),
                Facility.FacilityCategory.WATER_TAP => new Point(0, 1536),
                Facility.FacilityCategory.REFUSE_BIN => new Point(512, 1536),
                Facility.FacilityCategory.SAILMAKER => new Point(1024, 1536),
                Facility.FacilityCategory.LAUNDRETTE => new Point(1536, 1536),
                _ => throw new Exception("Missing enum switch case"),
            };
        }

        private class Facility
        {
            public Vector2 Coordinates { get; }
            public FacilityCategory Category { get; }

            /// <inheritdoc />
            public Facility(Vector2 coordinates, FacilityCategory category)
            {
                Coordinates = coordinates;
                Category = category;
            }

            [Flags]
            public enum FacilityCategory
            {
                NONE = 0,
                SLIPWAY = 1,
                VISITOR_BERTH = 2,
                BOAT_HOIST = 4,
                FUEL_STATION = 8,
                PUMP_OUT = 16,
                ELECTRICITY = 32,
                BOATYARD = 64,
                TOILETS = 128,
                CHANDLER = 256,
                SHOWERS = 512,
                NAUTICAL_CLUB = 1024,
                VISITORS_MOORING = 2048,
                WATER_TAP = 4096,
                REFUSE_BIN = 8192,
                SAILMAKER = 16384,
                LAUNDRETTE = 32768,
            }
        }
    }
}
