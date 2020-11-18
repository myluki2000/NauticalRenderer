using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Data;
using NauticalRenderer.Utility;
using OsmSharp.Streams;

namespace NauticalRenderer.SlippyMap.Layers
{
    class NavigationLineLayer : MapLayer
    {
        private (RectangleF boundingRect, Vector2[] points)[] navigationLines;

        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            navigationLines = OsmHelpers.WaysToListOfVector2Arr(new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete()
                .Where(osmGeo => osmGeo.Tags.Contains("seamark:type", "navigation_line"))).Select(x => (OsmHelpers.GetBoundingRectOfPoints(x), x)).ToArray();
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
                        LineRenderer.DrawDashedLine(mapSb,
                            points,
                            Color.Black,
                            new[] { 15 / camera.Scale.Y,
                                15 / camera.Scale.Y },
                            camera.GetMatrix());
                    }
                }
            }
        }
    }
}
