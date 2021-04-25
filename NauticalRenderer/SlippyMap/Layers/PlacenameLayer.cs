using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using NauticalRenderer.Data;
using NauticalRenderer.Data.MapPack;
using NauticalRenderer.Utility;
using OsmSharp.Complete;
using OsmSharp.Streams;
using OsmSharp.Streams.Complete;

namespace NauticalRenderer.SlippyMap.Layers
{
    class PlacenameLayer : MapLayer
    {
        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        private (Vector2 coords, string name)[] places;

        private (Vector2 coords, string name)[] cities;
        private (Vector2 coords, string name)[] towns;
        private (Vector2 coords, string name)[] villages;

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            OsmCompleteStreamSource source = new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete();
            places = source
                .Where(x => x.Tags.ContainsKey("place") && x.Tags.ContainsKey("name"))
                .Select(x => (OsmHelpers.GetCoordinateOfOsmGeo(x), x.Tags["name"]))
                .ToArray();

            OsmCompleteStreamSource placenameSource = new PBFOsmStreamSource(mapPack.OpenFile("placenames.osm.pbf")).ToComplete();
            cities = placenameSource
                .Where(x => x.Tags.Contains("place", "city") && x.Tags.ContainsKey("name"))
                .Select(x => (OsmHelpers.GetCoordinateOfOsmGeo(x), x.Tags["name"]))
                .ToArray();

            towns = placenameSource
                .Where(x => x.Tags.Contains("place", "town") && x.Tags.ContainsKey("name"))
                .Select(x => (OsmHelpers.GetCoordinateOfOsmGeo(x), x.Tags["name"]))
                .ToArray();

            villages = placenameSource
                .Where(x => x.Tags.Contains("place", "village") && x.Tags.ContainsKey("name"))
                .Select(x => (OsmHelpers.GetCoordinateOfOsmGeo(x), x.Tags["name"]))
                .ToArray();
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            DrawPlaces(cities, sb, camera);

            if(camera.Scale.Y > 6000)
                DrawPlaces(places, sb, camera);

            if (camera.Scale.Y > 10000)
                DrawPlaces(towns, sb, camera);

            if(camera.Scale.Y > 20000)
                DrawPlaces(villages, sb, camera);
            
        }

        private void DrawPlaces((Vector2 coords, string name)[] places, SpriteBatch sb, Camera camera)
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
