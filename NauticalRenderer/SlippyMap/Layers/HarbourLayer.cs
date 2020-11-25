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
using NauticalRenderer.Screens;
using NauticalRenderer.SlippyMap.UI;
using NauticalRenderer.Utility;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Streams;

namespace NauticalRenderer.SlippyMap.Layers
{
    class HarbourLayer : MapLayer
    {
        private MapScreen mapScreen;
        
        private List<Harbour> harbours;

        /// <inheritdoc />
        public override ILayerSettings LayerSettings => harbourLayerSettings;
        private readonly HarbourLayerSettings harbourLayerSettings = new HarbourLayerSettings();

        /// <inheritdoc />
        public HarbourLayer(MapScreen mapScreen)
        {
            this.mapScreen = mapScreen;
        }

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            IEnumerable<ICompleteOsmGeo> harboursOsm =
                (from osmGeo in new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete()
                 where osmGeo.Tags.Contains("leisure", "marina") || osmGeo.Tags.Contains("seamark:type", "harbour") || osmGeo.Tags.ContainsKey("seamark:harbour:category")
                 select osmGeo);

            harbours = harboursOsm.Select(x =>
            {
                string categoryString = x.Tags.GetValue("seamark:harbour:category",
                    x.Tags.Contains("leisure", "marina") ? "marina" : "other");
                Enum.TryParse(categoryString.ToUpper(), out Harbour.HarbourCategory category);
                return new Harbour(category, x);
            }).ToList();
        }

        private MouseState lastMouseState;
        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            foreach (Harbour harbour in harbours)
            {
                Vector2 iconPos = OsmHelpers.GetCoordinateOfOsmGeo(harbour.OsmData).Transform(camera.GetMatrix()).Rounded();

                // draw icon
                Texture2D icon;
                switch (harbour.Category)
                {
                    case Harbour.HarbourCategory.FISHING:
                        if (!harbourLayerSettings.FishingHarboursVisible) continue;
                        icon = Icons.Harbours.Fishing;
                        break;
                    case Harbour.HarbourCategory.MARINA:
                        if (!harbourLayerSettings.MarinasVisible) continue;
                        icon = Icons.Harbours.Marina;
                        break;
                    case Harbour.HarbourCategory.MARINA_NO_FACILITIES:
                        if (!harbourLayerSettings.MarinasVisible) continue;
                        icon = Icons.Harbours.MarinaNoFacilities;
                        break;
                    default:
                        if (!harbourLayerSettings.HarboursVisible) continue;
                        icon = Icons.Harbours.Harbour;
                        break;
                }
                Vector2 iconSize = new Vector2(25, 25);
                sb.Draw(icon,
                    new Rectangle(iconPos.ToPoint(), iconSize.ToPoint()),
                    null,
                    Color.White,
                    0,
                    new Vector2(icon.Width / 2, icon.Height / 2), 
                    SpriteEffects.None,
                    0);

                // draw label on hover
                if (harbour.OsmData.Tags.TryGetValue("name", out string name) &&
                    new Rectangle((iconPos - iconSize / 2).ToPoint(), iconSize.ToPoint()).Contains(Mouse.GetState()
                        .Position))
                {
                    sb.DrawString(Fonts.Arial.Regular,
                        name,
                        iconPos,
                        Color.Black,
                        0,
                        Vector2.Zero,
                        1f,
                        SpriteEffects.None,
                        0f);

                    if (Mouse.GetState().LeftButton == ButtonState.Released
                        && lastMouseState.LeftButton == ButtonState.Pressed
                        && !mapScreen.desktop.IsMouseOverGUI
                        && mapScreen.desktop.GetWindows().All(x => x.Title != name))
                    {
                        Window window = new Window()
                        {
                            Title = name,
                            MaxWidth = 400,
                        };

                        OsmTagGrid tagGrid = new OsmTagGrid {TagsCollection = harbour.OsmData.Tags};

                        window.Content = tagGrid;
                        window.ShowModal(mapScreen.desktop);
                        
                        // prevent one click from opening multiple windows
                        return;
                    }
                }
            }

            lastMouseState = Mouse.GetState();
        }

        struct Harbour
        {
            public HarbourCategory Category { get; }
            public ICompleteOsmGeo OsmData { get; }

            public Harbour(HarbourCategory category, ICompleteOsmGeo osmData)
            {
                Category = category;
                OsmData = osmData;
            }

            public enum HarbourCategory
            {
                FISHING,
                MARINA,
                MARINA_NO_FACILITIES,
                OTHER
            }
        }

        public class HarbourLayerSettings : ILayerSettings
        {
            public bool HarboursVisible = true;
            public bool FishingHarboursVisible = true;
            public bool MarinasVisible = true;
        }
    }
}
