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
using NauticalRenderer.Graphics.Effects;
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
        private (int firstIndexDashed, VertexBuffer vertexBuffer)[] vbfSmallStreets;
        private int minLon;

        /// <inheritdoc />
        public override ILayerSettings LayerSettings => layerSettings;

        private StreetLayerSettings layerSettings = new();

        private readonly List<LineText> lineTextsMotorwayTrunk = new List<LineText>(); 
        private readonly List<LineText> lineTextsPrimary = new List<LineText>(); 
        private readonly List<LineText> lineTextsSecondary = new List<LineText>(); 
        private readonly List<LineText> lineTextsTertiary = new List<LineText>(); 
        private readonly List<LineText> lineTextsOther = new List<LineText>(); 

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            OsmCompleteStreamSource source = new PBFOsmStreamSource(mapPack.OpenFile("streets.osm.pbf")).ToComplete();

            List<VertexPositionColor> streetsList = new();
            List<VertexPositionColor> smallStreetsList = new();
            List<VertexPositionColor> dashedSmallStreetsList = new();

            foreach (ICompleteOsmGeo geo in source)
            {
                if(!(geo is CompleteWay way)) continue;

                if(!way.Tags.TryGetValue("highway", out string type)) continue;
                Color color = GetColorForHighwayType(type) * 0.7f;

                if (type is "path" or "footway" or "track" or "cycleway")
                {
                    dashedSmallStreetsList.AddRange(Utility.Utility.LineStripToLineList(OsmHelpers.WayToLineStrip(way))
                        .Select(x => new VertexPositionColor(new Vector3(x, 0), color)));
                }
                else
                {
                    List<VertexPositionColor> list = streetsList;

                    if (type is "residential" or "service" or "living_street" or "tertiary_link" or "secondary_link"
                        or "primary_link" or "cycleway" or "unclassified")
                        list = smallStreetsList;

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
            vbfSmallStreets = new (int firstIndexDashed, VertexBuffer vertexBuffer)[maxLon - minLon];
            for (int lonI = 0; lonI < maxLon - minLon; lonI++)
            {
                List<VertexPositionColor> points = new();
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

                vbfSmallStreets[lonI].firstIndexDashed = points.Count;

                // add dashed streets after that
                for (int i = 0; i < dashedSmallStreetsList.Count - 1; i += 2)
                {
                    VertexPositionColor p1 = dashedSmallStreetsList[i];
                    VertexPositionColor p2 = dashedSmallStreetsList[i + 1];

                    // if both points lie outside the bounds and on the same side the line does not intersect the area
                    if (p1.Position.X < minLon + lonI && p2.Position.X < minLon + lonI
                        || p1.Position.X > minLon + lonI + 1 && p2.Position.X > minLon + lonI + 1) continue;

                    points.Add(p1);
                    points.Add(p2);
                }

                if (points.Count > 0)
                {
                    vbfSmallStreets[lonI].vertexBuffer = new VertexBuffer(Globals.Graphics.GraphicsDevice, typeof(VertexPositionColor), points.Count, BufferUsage.WriteOnly);
                    vbfSmallStreets[lonI].vertexBuffer.SetData(points.ToArray());
                }
                else
                {
                    vbfSmallStreets[lonI].vertexBuffer = null;
                }
            }

            
            vbfStreets = new VertexBuffer(Globals.Graphics.GraphicsDevice, typeof(VertexPositionColor), streetsList.Count, BufferUsage.WriteOnly);
            vbfStreets.SetData(streetsList.ToArray());
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            DashedLineEffect.WorldMatrix = camera.GetMatrix();
            DashedLineEffect.LineAndGapLengths = new[] { 5f, 5f, 0f, 0f };
            EffectPool.BasicEffect.View = camera.GetMatrix();

            // draw small streets
            if (layerSettings.MinorStreetsVisible && camera.Scale.Y > 10000)
            {
                int screenLeftLon = (int)camera.DrawBounds.X;
                int screenRightLon = (int) Math.Ceiling(camera.DrawBounds.Right);

                for (int i = screenLeftLon - minLon; i < screenRightLon - minLon; i++)
                {
                    if (vbfSmallStreets[i].vertexBuffer == null) continue;

                    sb.GraphicsDevice.SetVertexBuffer(vbfSmallStreets[i].vertexBuffer);


                    if (camera.Scale.Y > 30000)
                    {
                        // draw dashed lines
                        DashedLineEffect.Apply();
                        sb.GraphicsDevice.DrawPrimitives(PrimitiveType.LineList,
                            vbfSmallStreets[i].firstIndexDashed,
                            (vbfSmallStreets[i].vertexBuffer.VertexCount - vbfSmallStreets[i].firstIndexDashed) / 2);
                    }

                    // draw lines
                    EffectPool.BasicEffect.CurrentTechnique.Passes[0].Apply();
                    if (vbfSmallStreets[i].firstIndexDashed != 0)
                        sb.GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, vbfSmallStreets[i].firstIndexDashed / 2);
                }
            }

            // draw major streets
            if (layerSettings.MajorStreetsVisible)
            {
                EffectPool.BasicEffect.CurrentTechnique.Passes[0].Apply();
                sb.GraphicsDevice.SetVertexBuffer(vbfStreets);
                sb.GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, vbfStreets.VertexCount / 2);

                if (camera.Scale.Y > 80000)
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
            }

            if (layerSettings.MinorStreetsVisible && camera.Scale.Y > 200000)
            {
                foreach (LineText lineText in lineTextsOther)
                {
                    lineText.Draw(sb, Color.Black, camera);
                }
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
                case "cycleway":
                    return Color.LightBlue;
                default:
                    return Color.Gray;
            }
        }

        public class StreetLayerSettings : ILayerSettings
        {
            public bool MinorStreetsVisible = true;
            public bool MajorStreetsVisible = true;
        }
    }
}
