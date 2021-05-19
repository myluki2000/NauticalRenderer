using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Data;
using NauticalRenderer.Data.MapPack;
using NauticalRenderer.SlippyMap.UI;
using NauticalRenderer.Utility;
using OsmSharp.Complete;
using OsmSharp.Streams;
using OsmSharp.Tags;

namespace NauticalRenderer.SlippyMap.Layers
{
    class NavigationLineLayer : MapLayer
    {
        private (RectangleF boundingRect, Vector2[] points)[] navigationLines;
        private VertexPositionColor[] recommendedTracks;
        private LineText[] lineLabels;

        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            List<LineText> labels = new List<LineText>();

            navigationLines = new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete()
                .OfType<CompleteWay>()
                .Where(osmGeo => osmGeo.Tags.Contains("seamark:type", "navigation_line"))
                .Select(x =>
                {
                    Vector2[] points = OsmHelpers.WayToLineStrip(x);

                    foreach (Tag tag in x.Tags)
                    {
                        if (tag.Key.EndsWith("orientation"))
                        {
                            labels.Add(new LineText(points, tag.Value + "°", Myra.DefaultAssets.FontSmall, LineText.Alignment.CENTER));
                            break;
                        }
                    }
                    return (OsmHelpers.GetBoundingRectOfPoints(points), points);
                })
                .ToArray();

            recommendedTracks = new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete()
                .OfType<CompleteWay>()
                .Where(osmGeo => osmGeo.Tags.Contains("seamark:type", "recommended_track"))
                .Select(x =>
                {
                    Vector2[] points = OsmHelpers.WayToLineStrip(x);

                    foreach (Tag tag in x.Tags)
                    {
                        if (tag.Key.EndsWith("orientation"))
                        {
                            labels.Add(new LineText(points, tag.Value + "°", Myra.DefaultAssets.FontSmall, LineText.Alignment.CENTER));
                            break;
                        }
                    }
                    return points;
                })
                .SelectMany(x => Utility.Utility.LineStripToLineList(x).Select(x => new VertexPositionColor(new Vector3(x, 0), Color.Black)))
                .ToArray();

            lineLabels = labels.ToArray();
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            if (camera.Scale.Y > 10000)
            {
                foreach ((RectangleF boundingRect, Vector2[] points) in navigationLines)
                {
                    if (boundingRect.Intersects(camera.DrawBounds))
                    {
                        LineRenderer.DrawDashedLine(
                            mapSb,
                            points,
                            Color.Black,
                            new[] { 5f, 5f, 0, 0 },
                            camera.GetMatrix());
                    }
                }

                LineRenderer.DrawLineList(sb, recommendedTracks, camera.GetMatrix());

                foreach (LineText label in lineLabels)
                {
                    label.Draw(sb, Color.Black, camera);
                }
            }
        }
    }
}
