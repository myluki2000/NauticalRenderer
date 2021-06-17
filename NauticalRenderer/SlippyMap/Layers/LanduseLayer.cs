using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Data;
using NauticalRenderer.Data.MapPack;
using NauticalRenderer.Graphics;
using NauticalRenderer.SlippyMap.Data;
using NauticalRenderer.Utility;
using OsmSharp.Complete;
using OsmSharp.Streams;

namespace NauticalRenderer.SlippyMap.Layers
{
    class LanduseLayer : MapLayer
    {
        /// <inheritdoc />
        public override ILayerSettings LayerSettings => landuseLayerSettings;

        private LanduseLayerSettings landuseLayerSettings = new();

        private Mesh forestsMesh;
        private Mesh buildingsMesh;

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            IEnumerable<ICompleteOsmGeo> geos = new PBFOsmStreamSource(mapPack.OpenFile("landuse.osm.pbf")).ToComplete()
                .Where(x => x is CompleteWay way && way.Nodes[0] == way.Nodes[^1]);

            List<Vector2> forestVerts = new List<Vector2>();
            List<Vector2> buildingVerts = new List<Vector2>();
            foreach (ICompleteOsmGeo geo in geos)
            {
                Vector2[] points = ((CompleteWay) geo).Nodes.Select(OsmHelpers.GetCoordinateOfOsmGeo).ToArray();

                switch (geo.Tags["landuse"].ToLowerInvariant())
                {
                    case "forest":
                        forestVerts.AddRange(Utility.Utility.Triangulate(points));
                        break;
                    case "industrial":
                    case "residential":
                    case "retail":
                    case "commercial":
                        buildingVerts.AddRange(Utility.Utility.Triangulate(points));
                        break;
                    default:
                        throw new Exception("Found landuse value that not defined: " + geo.Tags["landuse"].ToLowerInvariant());
                }
            }

            forestsMesh = new Mesh(forestVerts.ToArray(), MapStyle.COLOR_FOREST);
            buildingsMesh = new Mesh(buildingVerts.ToArray(), MapStyle.COLOR_BUILDINGS);
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            if (!landuseLayerSettings.LandusesVisible) return;

            forestsMesh.Draw(sb, camera.GetMatrix());
            buildingsMesh.Draw(sb, camera.GetMatrix());
        }

        private class LanduseLayerSettings : ILayerSettings
        {
            public bool LandusesVisible = true;
        }
    }
}
