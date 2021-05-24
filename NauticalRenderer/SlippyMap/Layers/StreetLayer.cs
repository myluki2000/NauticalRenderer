using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Data;
using NauticalRenderer.Data.MapPack;
using NauticalRenderer.Graphics;
using NauticalRenderer.SlippyMap.UI;
using NauticalRenderer.Utility;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Streams;
using OsmSharp.Streams.Complete;

namespace NauticalRenderer.SlippyMap.Layers
{
    class StreetLayer : MapLayer
    {
        private VertexBuffer vbfStreets;
        private VertexBuffer[] vbfSmallStreets;
        private int minLon;
        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        private readonly List<LineText> lineTextsMotorwayTrunk = new List<LineText>(); 
        private readonly List<LineText> lineTextsPrimary = new List<LineText>(); 
        private readonly List<LineText> lineTextsSecondary = new List<LineText>(); 
        private readonly List<LineText> lineTextsTertiary = new List<LineText>(); 
        private readonly List<LineText> lineTextsOther = new List<LineText>(); 

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
                    smallStreetsList.AddRange(LineRenderer.GenerateDashedLineVerts(OsmHelpers.WayToLineStrip(way), color, new []{0.0001f, 0.0001f}));
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
                        switch (type)
                        {
                            case "motorway":
                            case "trunk":
                                lineTextsMotorwayTrunk.Add(new LineText(lineStrip, geo.Tags["name"], Myra.DefaultAssets.FontSmall, LineText.Alignment.CENTER));
                                break;
                            case "primary":
                                lineTextsPrimary.Add(new LineText(lineStrip, geo.Tags["name"], Myra.DefaultAssets.FontSmall, LineText.Alignment.CENTER));
                                break;
                            case "secondary":
                                lineTextsSecondary.Add(new LineText(lineStrip, geo.Tags["name"], Myra.DefaultAssets.FontSmall, LineText.Alignment.CENTER));
                                break;
                            case "tertiary":
                                lineTextsTertiary.Add(new LineText(lineStrip, geo.Tags["name"], Myra.DefaultAssets.FontSmall, LineText.Alignment.CENTER));
                                break;
                            default:
                                lineTextsOther.Add(new LineText(lineStrip, geo.Tags["name"], Myra.DefaultAssets.FontSmall, LineText.Alignment.CENTER));
                                break;
                        }
                        
                    }
                }
            }

            minLon = (int)mapPack.BoundingPolygon.Min(x => x.X);
            int maxLon = (int) Math.Ceiling(mapPack.BoundingPolygon.Max(x => x.X));
            vbfSmallStreets = new VertexBuffer[maxLon - minLon];
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

                if (points.Count > 0)
                {
                    vbfSmallStreets[lonI] = new VertexBuffer(Globals.Graphics.GraphicsDevice, typeof(VertexPositionColor), points.Count, BufferUsage.WriteOnly);
                    vbfSmallStreets[lonI].SetData(points.ToArray());
                }
                else
                {
                    vbfSmallStreets[lonI] = null;
                }
            }

            
            vbfStreets = new VertexBuffer(Globals.Graphics.GraphicsDevice, typeof(VertexPositionColor), streetsList.Count, BufferUsage.WriteOnly);
            vbfStreets.SetData(streetsList.ToArray());
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            EffectPool.BasicEffect.View = camera.GetMatrix();
            EffectPool.BasicEffect.CurrentTechnique.Passes[0].Apply();

            // draw small streets
            if (camera.Scale.Y > 10000)
            {
                int screenLeftLon = (int)camera.DrawBounds.X;
                int screenRightLon = (int) Math.Ceiling(camera.DrawBounds.Right);

                for (int i = screenLeftLon - minLon; i < screenRightLon - minLon; i++)
                {
                    if (vbfSmallStreets[i] == null) continue;

                    sb.GraphicsDevice.SetVertexBuffer(vbfSmallStreets[i]);
                    sb.GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, vbfSmallStreets[i].VertexCount / 2);
                }
            }
                
            
            // draw major streets
            sb.GraphicsDevice.SetVertexBuffer(vbfStreets);
            sb.GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, vbfStreets.VertexCount / 2);



            if(camera.Scale.Y > 80000)
                foreach (LineText lineText in lineTextsMotorwayTrunk)
                {
                        lineText.Draw(sb, Color.Black, camera);
                }

            if (camera.Scale.Y > 100000)
                foreach (LineText lineText in lineTextsPrimary)
                {
                    lineText.Draw(sb, Color.Black, camera);
                }

            if (camera.Scale.Y > 120000)
                foreach (LineText lineText in lineTextsSecondary)
                {
                    lineText.Draw(sb, Color.Black, camera);
                }

            if (camera.Scale.Y > 150000)
                foreach (LineText lineText in lineTextsTertiary)
                {
                    lineText.Draw(sb, Color.Black, camera);
                }

            if (camera.Scale.Y > 200000)
                foreach (LineText lineText in lineTextsOther)
                {
                    lineText.Draw(sb, Color.Black, camera);
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
