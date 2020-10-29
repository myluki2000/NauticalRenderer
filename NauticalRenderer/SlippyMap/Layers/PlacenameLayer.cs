using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using NauticalRenderer.Data;
using NauticalRenderer.Utility;
using OsmSharp.Streams;
using OsmSharp.Streams.Complete;

namespace NauticalRenderer.SlippyMap.Layers
{
    class PlacenameLayer : MapLayer
    {
        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        private (Vector2 coords, string name)[] places;

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            OsmCompleteStreamSource source = new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete();
            places = source
                .Where(x => x.Tags.ContainsKey("place") && x.Tags.ContainsKey("name"))
                .Select(x => (OsmHelpers.GetCoordinateOfOsmGeo(x), x.Tags["name"]))
                .ToArray();


            //OsmCompleteStreamSource placenameSource = new PBFOsmStreamSource(Globals.ResourceManager.GetStreamForFile("Content/sh-placenames.osm.pbf")).ToComplete();
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            foreach ((Vector2 coords, string name) in places)
            {
                SpriteFont font = Myra.DefaultAssets.FontSmall;
                string formattedLabel = Utility.Utility.WrapText(font, name, 100);
                Vector2 labelSize = font.MeasureString(formattedLabel);
                sb.DrawString(font, formattedLabel, coords.Transform(camera.GetMatrix()).Rounded(), Color.Black, 0, (labelSize / 2).Rounded(), Vector2.One, SpriteEffects.None, 0);
            }
        }
    }
}
