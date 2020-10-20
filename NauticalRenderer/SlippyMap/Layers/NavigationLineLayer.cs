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
        private List<Vector2[]> navigationLines;

        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            navigationLines = OsmHelpers.WaysToListOfVector2Arr(
                from osmGeo in new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete()
                where osmGeo.Tags.Contains("seamark:type", "navigation_line")
                select osmGeo);
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            if (camera.Scale.Y > 10000)
            {
                foreach (Vector2[] line in navigationLines)
                {
                    LineRenderer.DrawDashedLine(mapSb,
                        line,
                        Color.Black,
                        new[] { 15 / camera.Scale.Y,
                                15 / camera.Scale.Y },
                        camera.GetMatrix());
                }
            }
        }
    }
}
