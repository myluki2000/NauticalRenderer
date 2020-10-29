using System;
using System.Collections.Generic;
using System.Linq;
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

        private Effect facilitiesEffect;

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            facilitiesEffect = Globals.Content.Load<Effect>("Effects/FacilitiesEffect");

            List<Facility> facilites = new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete()
                .Where(x => x.Tags.ContainsKey("seamark:small_craft_facility:category"))
                .Select(x =>
                {
                    string[] categories = x.Tags["seamark:small_craft_facility:category"].Split(';');
                    Facility.FacilityCategory category = Facility.FacilityCategory.NONE;
                    foreach (string s in categories)
                    {
                        Enum.TryParse(s, true, out Facility.FacilityCategory nc);
                        category |= nc;
                    }

                    return new Facility(OsmHelpers.GetCoordinateOfOsmGeo(x), category);
                }).ToList();
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            throw new NotImplementedException();
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
