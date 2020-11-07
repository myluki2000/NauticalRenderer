using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Data;
using NauticalRenderer.Utility;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Streams;
using OsmSharp.Streams.Complete;

namespace NauticalRenderer.SlippyMap.Layers
{
    class StreetLayer : MapLayer
    {
        private VertexPositionColor[] streets;
        private VertexPositionColor[] smallStreets;

        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            OsmCompleteStreamSource source = new PBFOsmStreamSource(mapPack.OpenFile("streets.osm.pbf")).ToComplete();

            List<VertexPositionColor> streetsList = new List<VertexPositionColor>();
            List<VertexPositionColor> smallStreetsList = new List<VertexPositionColor>();

            foreach (ICompleteOsmGeo geo in source)
            {
                if(!(geo is CompleteWay way)) continue;

                if(!way.Tags.TryGetValue("highway", out string type)) continue;
                Color color = GetColorForHighwayType(type) * 0.7f;

                if (type == "track" || type == "path" || type == "footway")
                {
                    smallStreetsList.AddRange(LineRenderer.GenerateDashedLineVerts(OsmHelpers.WayToVector2Arr(way), color, new []{0.0001f, 0.0001f}));
                }
                else
                {
                    streetsList.Add(new VertexPositionColor(new Vector3(OsmHelpers.GetCoordinateOfOsmGeo(way.Nodes[0]), 0), color));
                    for (int i = 1; i < way.Nodes.Length - 1; i++)
                    {
                        streetsList.Add(new VertexPositionColor(new Vector3(OsmHelpers.GetCoordinateOfOsmGeo(way.Nodes[i]), 0), color));
                        streetsList.Add(new VertexPositionColor(new Vector3(OsmHelpers.GetCoordinateOfOsmGeo(way.Nodes[i]), 0), color));
                    }
                    streetsList.Add(new VertexPositionColor(new Vector3(OsmHelpers.GetCoordinateOfOsmGeo(way.Nodes[^1]), 0), color));
                }
            }

            
            streets = streetsList.ToArray();
            smallStreets = smallStreetsList.ToArray();
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            if(camera.Scale.Y > 15000)
                LineRenderer.DrawLineList(mapSb, smallStreets, camera.GetMatrix());
            LineRenderer.DrawLineList(mapSb, streets, camera.GetMatrix());
        }

        private static Color GetColorForHighwayType(string type)
        {
            switch (type)
            {
                case "motorway":
                case "motorway_link":
                    return Color.Blue;
                case "trunk":
                case "trunk_link":
                    return Color.Red;
                case "primary":
                case "primary_link":
                    return Color.Orange;
                case "secondary":
                case "secondary_link":
                    return Color.Yellow;
                case "tertiary":
                case "tertiary_link":
                    return Color.White;
                case "track":
                    return Color.Brown;
                case "path":
                case "footway":
                    return Color.Red;
                default:
                    return Color.Gray;
            }
        }
    }
}
