using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using NauticalRenderer.Data;
using NauticalRenderer.Data.MapPack;
using NauticalRenderer.Graphics;
using NauticalRenderer.Graphics.Effects;
using NauticalRenderer.Input;
using NauticalRenderer.Resources;
using NauticalRenderer.Screens;
using NauticalRenderer.Utility;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Streams;
using OsmSharp.Tags;

namespace NauticalRenderer.SlippyMap.Layers
{
    class HarbourLayer : MapLayer
    {
        private readonly MapScreen mapScreen;

        private readonly List<Harbour> harbours = new();

        /// <inheritdoc />
        public override ILayerSettings LayerSettings => harbourLayerSettings;
        private readonly HarbourLayerSettings harbourLayerSettings = new HarbourLayerSettings();

        private SymbolInstancingEffectData effectData;

        /// <inheritdoc />
        public HarbourLayer(MapScreen mapScreen)
        {
            this.mapScreen = mapScreen;
        }

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            harbours.Clear();

            IEnumerable<ICompleteOsmGeo> harboursOsm =
                (from osmGeo in new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete()
                 where osmGeo.Tags.Contains("leisure", "marina") || osmGeo.Tags.Contains("seamark:type", "harbour") || osmGeo.Tags.ContainsKey("seamark:harbour:category")
                 select osmGeo);

            foreach (ICompleteOsmGeo geo in harboursOsm)
            {
                string categoryString = geo.Tags.GetValueOrDefault("seamark:harbour:category",
                    geo.Tags.Contains("leisure", "marina") ? "marina" : "other");
                if (!Enum.TryParse(categoryString, true, out Harbour.HarbourCategory category))
                    category = Harbour.HarbourCategory.OTHER;

                Harbour h = new(category, OsmHelpers.GetCoordinateOfOsmGeo(geo), geo.Tags);
                harbours.Add(h);

            }

            effectData = new();
            effectData.SetInstanceData(harbours.Select(x =>
            {
                Vector2 atlasCoords = x.Category switch
                {
                    Harbour.HarbourCategory.FISHING => Icons.Harbours.Fishing.Location.ToVector2(),
                    Harbour.HarbourCategory.MARINA => Icons.Harbours.Marina.Location.ToVector2(),
                    Harbour.HarbourCategory.MARINA_NO_FACILITIES => Icons.Harbours.MarinaNoFacilities.Location.ToVector2(),
                    _ => Icons.Harbours.Harbour.Location.ToVector2()
                };

                atlasCoords /= Icons.Harbours.Texture.Width;

                return new SymbolInstancingEffectData.InstanceInfo(x.Coordinates, atlasCoords);
            }).ToArray());
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            if (!harbourLayerSettings.HarboursVisible) return;

            float iconSize = 26;
            if (camera.Scale.Y < 10000) iconSize = 20;

            SymbolInstancingEffect.Size = iconSize;
            SymbolInstancingEffect.WorldMatrix = camera.GetMatrix();
            SymbolInstancingEffect.Texture = Icons.Harbours.Texture;
            SymbolInstancingEffect.AtlasWidth = 2;
            effectData.Draw();

            foreach (Harbour harbour in harbours)
            {
                Vector2 iconPos = harbour.Coordinates.Transform(camera.GetMatrix()).Rounded();

                RectangleF boundingRect = new(
                    iconPos.X - iconSize / 2,
                    iconPos.Y - iconSize / 2,
                    iconSize,
                    iconSize);

                // draw label on hover
                if (boundingRect.Contains(Mouse.GetState().Position.ToVector2()))
                {
                    if (harbour.Tags.TryGetValue("name", out string name))
                        sb.DrawString(Fonts.Arial.Regular,
                            name,
                            iconPos,
                            Color.Black,
                            0,
                            Vector2.Zero,
                            1f,
                            SpriteEffects.None,
                            0f);

                    if (MouseHelper.HasUnhandledLeftClick && !mapScreen.Desktop.IsMouseOverGUI)
                    {
                        if (!mapScreen.ShowOsmTagsWindow(name, harbour.Tags)) continue; // window was not opened

                        // window was opened
                        MouseHelper.LeftClickWasHandled();
                        break;

                    }
                }
            }
        }

        struct Harbour
        {
            public HarbourCategory Category { get; }
            public Vector2 Coordinates { get; }
            public TagsCollectionBase Tags { get; }

            public Harbour(HarbourCategory category, Vector2 coordinates, TagsCollectionBase tags)
            {
                Category = category;
                Coordinates = coordinates;
                Tags = tags;
            }

            public enum HarbourCategory
            {
                OTHER,
                FISHING,
                MARINA,
                MARINA_NO_FACILITIES,
            }
        }

        public class HarbourLayerSettings : ILayerSettings
        {
            public bool HarboursVisible = true;
        }
    }
}
