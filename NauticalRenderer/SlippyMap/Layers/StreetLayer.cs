using System;
using System.CodeDom;
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
        private VertexPositionColor[][] smallStreets;
        private int minLon;
        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        private readonly List<(int minZoom, LineText lineText)> lineTexts = new List<(int minZoom, LineText lineText)>(); 

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
                    List<VertexPositionColor> list = streetsList;

                    if (type == "residential" 
                        || type == "service" 
                        || type == "living_street"
                        || type == "tertiary_link"
                        || type == "secondary_link"
                        || type == "primary_link"
                        || type == "unclassified") list = smallStreetsList;

                    Vector2[] lineStrip = way.Nodes.Select(OsmHelpers.GetCoordinateOfOsmGeo).ToArray();
                    list.AddRange(Utility.Utility.LineStripToLineList(lineStrip).Select(x => new VertexPositionColor(new Vector3(x, 0), color)));
                    
                    if (geo.Tags.ContainsKey("name"))
                    {
                        int minZoom = 200000;
                        switch (type)
                        {
                            case "motorway":
                            case "trunk":
                                minZoom = 80000;
                                break;
                            case "primary":
                                minZoom = 100000;
                                break;
                            case "secondary":
                                minZoom = 120000;
                                break;
                            case "tertiary":
                                minZoom = 150000;
                                break;
                        }
                        lineTexts.Add((minZoom, new LineText(lineStrip, geo.Tags["name"], Myra.DefaultAssets.FontSmall, LineText.Alignment.CENTER)));
                    }
                }
            }

            minLon = (int)mapPack.BoundingPolygon.Min(x => x.X);
            int maxLon = (int) Math.Ceiling(mapPack.BoundingPolygon.Max(x => x.X));
            smallStreets = new VertexPositionColor[maxLon - minLon][];
            for (int lonI = 0; lonI < maxLon - minLon; lonI++)
            {
                List<VertexPositionColor> points = new List<VertexPositionColor>();
                for (int i = 0; i < smallStreetsList.Count - 1; i += 2)
                {
                    VertexPositionColor p1 = smallStreetsList[i];
                    VertexPositionColor p2 = smallStreetsList[i + 1];

                    // if both points lie outside the bounds and on the same side the line does not intersect the area
                    if (p1.Position.X < minLon + lonI && p2.Position.X < minLon + lonI
                        || p1.Position.X > minLon + lonI + 1 && p2.Position.X > minLon + lonI + 1) continue;

                    points.Add(p1);
                    points.Add(p2);
                }

                smallStreets[lonI] = points.ToArray();
            }

            
            streets = streetsList.ToArray();
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            if (camera.Scale.Y > 10000)
            {
                int screenLeftLon = (int)camera.DrawBounds.X;
                int screenRightLon = (int) Math.Ceiling(camera.DrawBounds.Right);

                for (int i = screenLeftLon - minLon; i < screenRightLon - minLon; i++)
                {
                    LineRenderer.DrawLineList(mapSb, smallStreets[i], camera.GetMatrix());
                }
            }
                
            LineRenderer.DrawLineList(mapSb, streets, camera.GetMatrix());

            if(camera.Scale.Y > 80000)
                foreach ((int minZoom, LineText lineText) in lineTexts)
                {
                    if(lineText.BoundingRect.Intersects(camera.DrawBounds) && camera.Scale.Y > minZoom)
                        lineText.Draw(sb, Color.Black, camera.GetMatrix());
                }

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
