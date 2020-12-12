using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Data;
using NauticalRenderer.SlippyMap.Data;
using NauticalRenderer.Utility;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Streams;
using OsmSharp.Tags;

namespace NauticalRenderer.SlippyMap.Layers
{
    class LandmarkLayer : MapLayer
    {
        /// <inheritdoc />
        public override ILayerSettings LayerSettings => landmarkLayerSettings;
        private readonly LandmarkLayerSettings landmarkLayerSettings = new LandmarkLayerSettings();

        private Landmark[] landmarks;
        private List<ICompleteOsmGeo> lights;

        private SectorLight[] sectorLights;

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            lights = new List<ICompleteOsmGeo>();

            landmarks = (new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf"))
                .ToComplete()
                .Where(osmGeo =>
                    osmGeo.Tags.Contains("seamark:type", "landmark") ||
                    osmGeo.Tags.Contains("seamark:type", "light_major") ||
                    osmGeo.Tags.Contains("seamark:type", "light_minor")))
                .Where(o =>
                {
                    if (o.Tags.Contains("seamark:type", "landmark") &&
                        !o.Tags.ToList().Exists(x => x.Key.StartsWith("seamark:light")))
                        return true;

                    lights.Add(o);
                    return false;
                })
                .Select(x =>
                {
                    if(x.Tags.TryGetValue("seamark:landmark:category", out string categoryString))
                    {
                        if (Enum.TryParse(categoryString.ToUpper(), out Landmark.LandmarkCategory category))
                        {
                            return new Landmark(category, OsmHelpers.GetCoordinateOfOsmGeo(x));
                        }

                        return new Landmark(Landmark.LandmarkCategory.GENERIC, OsmHelpers.GetCoordinateOfOsmGeo(x));
                    }

                    return new Landmark(Landmark.LandmarkCategory.GENERIC, OsmHelpers.GetCoordinateOfOsmGeo(x));
                })
                .ToArray();

            sectorLights = lights.Where(x => !x.Tags.ContainsKey("seamark:light:colour")).Select(x =>
            {
                SectorLight sl = new SectorLight();

                sl.Major = !x.Tags.Contains("seamark:type", "light_minor");
                sl.Coordinates = OsmHelpers.GetCoordinateOfOsmGeo(x);

                List<SectorLight.Sector> sectors = new List<SectorLight.Sector>();
                int i = 1;
                while (true)
                {
                    SectorLight.Sector s = new SectorLight.Sector();
                    s.Orientation = float.NaN;
                    s.Range = 30;

                    Dictionary<string, Tag> tags = x.Tags.Where(x => x.Key.StartsWith("seamark:light:" + i + ":")).ToDictionary(x => x.Key.Replace("seamark:light:" + i + ":", ""));
                    i++;

                    // we have reached the end!
                    if (tags.Count == 0) break;
                    // we can't draw the light if we don't know the color
                    if (!tags.ContainsKey("colour")) continue;

                    if (tags.TryGetValue("range", out Tag rangeTag))
                        s.Range = float.Parse(rangeTag.Value, CultureInfo.InvariantCulture) * (s.Range / 10);

                    if (tags.ContainsKey("sector_start") && tags.ContainsKey("sector_end") && tags["sector_start"].Value != tags["sector_end"].Value)
                    {
                        s.StartAngle = Utility.Utility.NormalizeAngleRad(
                            MathHelper.ToRadians(float.Parse(tags["sector_start"].Value, CultureInfo.InvariantCulture)) - MathHelper.Pi);
                        s.EndAngle = Utility.Utility.NormalizeAngleRad(
                            MathHelper.ToRadians(float.Parse(tags["sector_end"].Value, CultureInfo.InvariantCulture)) - MathHelper.Pi);
                    }
                    else
                    {
                        // check if light has orientation instead
                        if (!tags.ContainsKey("orientation"))
                        {
                            // if not that means that sector_start and end are the same value so use these as orientation instead
                            if (tags.ContainsKey("sector_start") && tags.ContainsKey("sector_end"))
                            {
                                s.Orientation = float.Parse(tags["sector_start"].Value, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                continue;
                            }

                        }
                        else
                        {
                            s.Orientation = MathHelper.ToRadians(float.Parse(tags["orientation"].Value, CultureInfo.InvariantCulture)) - MathHelper.Pi;
                        }


                        s.StartAngle = Utility.Utility.NormalizeAngleRad(s.Orientation - 0.05f);
                        s.EndAngle = Utility.Utility.NormalizeAngleRad(s.Orientation + 0.05f);
                    }

                    s.Color = OsmHelpers.GetColorFromSeamarkColor(tags["colour"].Value);
                    if (s.Color == Color.White) s.Color = Color.Yellow;

                    sectors.Add(s);
                }

                sl.Sectors = sectors.ToArray();

                return sl;
            }).ToArray();
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            if (landmarkLayerSettings.LandmarksVisible)
            {
                foreach (Landmark l in landmarks)
                {
                    Vector2 screenPos = l.Coordinates.Transform(camera.GetMatrix());

                    if (camera.Scale.Y > 6500)
                    {
                        Rectangle srcRect = Rectangle.Empty;
                        switch (l.Category)
                        {
                            case Landmark.LandmarkCategory.CHIMNEY:
                                srcRect = Icons.Landmarks.Chimney;
                                break;
                            case Landmark.LandmarkCategory.CROSS:
                                srcRect = Icons.Landmarks.Cross;
                                break;
                            case Landmark.LandmarkCategory.DISH_AERIAL:
                                srcRect = Icons.Landmarks.DishAerial;
                                break;
                            case Landmark.LandmarkCategory.FLAGSTAFF:
                                srcRect = Icons.Landmarks.Flagstaff;
                                break;
                            case Landmark.LandmarkCategory.FLARE_STACK:
                                srcRect = Icons.Landmarks.FlareStack;
                                break;
                            case Landmark.LandmarkCategory.MAST:
                                srcRect = Icons.Landmarks.Mast;
                                break;
                            case Landmark.LandmarkCategory.RADAR_SCANNER:
                            case Landmark.LandmarkCategory.TOWER:
                                srcRect = Icons.Landmarks.Tower;
                                break;
                            case Landmark.LandmarkCategory.WINDMOTOR:
                                srcRect = Icons.Landmarks.WindTurbine;
                                break;
                            case Landmark.LandmarkCategory.WINDMILL:
                                srcRect = Icons.Landmarks.Windmill;
                                break;
                            case Landmark.LandmarkCategory.WINDSOCK:
                                srcRect = Icons.Landmarks.Windsock;
                                break;
                        }

                        if (srcRect != Rectangle.Empty)
                        {
                            sb.Draw(Icons.Landmarks.Texture,
                                new Rectangle(screenPos.ToPoint(), new Point(32, 32)),
                                srcRect,
                                Color.White,
                                0,
                                new Vector2(256, 460),
                                SpriteEffects.None,
                                0);
                        }
                        else
                        {
                            Utility.Utility.DrawRectangleF(sb, new RectangleF(screenPos.X, screenPos.Y, 5f, 5f), Color.Red);
                        }
                    }
                }
            }

            foreach (ICompleteOsmGeo o in lights)
            {
                Vector2 pos = OsmHelpers.GetCoordinateOfOsmGeo(o);

                if (!camera.DrawBounds.Contains(pos)) continue;

                pos = pos.Transform(camera.GetMatrix());

                bool minor = o.Tags.Contains("seamark:type", "light_minor");
                if (minor && !landmarkLayerSettings.MinorLightsVisible ||
                    !minor && !landmarkLayerSettings.MajorLightsVisible) continue;

                // draw major lights at zoom > 500, minor lights at zoom > 10000
                if ((!minor && camera.Scale.Y > 500) || camera.Scale.Y > 10000)
                {
                    // draw star
                    sb.Draw(Icons.Star, new Rectangle(pos.ToPoint(), !minor ? new Point(12, 12) : new Point(7, 7)), null, Color.White, 0,
                        new Vector2(Icons.Star.Width / 2, Icons.Star.Height / 2), SpriteEffects.None, 0);

                    DrawLight(sb, o, camera);

                    // draw name if zoomed in far enough
                    if (camera.Scale.Y > 5000)
                        DrawName(sb, o, camera);
                }
            }

            DrawSectorLights(sb, camera);
        }

        private void DrawName(SpriteBatch sb, ICompleteOsmGeo o, Camera camera)
        {
            if (!o.Tags.ContainsKey("seamark:name")) return;

            Vector2 pos = OsmHelpers.GetCoordinateOfOsmGeo(o);


            sb.DrawString(Myra.DefaultAssets.FontSmall,
                o.Tags["seamark:name"],
                pos.Transform(camera.GetMatrix()).Rounded(),
                Color.Black);
        }

        private void DrawLight(SpriteBatch sb, ICompleteOsmGeo o, Camera camera)
        {
            Color color = Color.DarkViolet; // default color for multi-color lights

            // check if object even has a light
            o.Tags.TryGetValue("seamark:light:colour", out string lightColor);
            if (lightColor != null)
            {
                // handle single color seamark light blob
                color = OsmHelpers.GetColorFromSeamarkColor(lightColor);
                if(color == Color.White) color = Color.Yellow;
            }


            Vector2 pos = OsmHelpers.GetCoordinateOfOsmGeo(o).Transform(camera.GetMatrix());

            sb.Draw(Icons.LightBlob, pos, null, color, MathHelper.ToRadians(135), new Vector2(Icons.LightBlob.Width / 2, Icons.LightBlob.Height), new Vector2(0.20f), SpriteEffects.None, 0f);
        }

        private void DrawSectorLights(SpriteBatch sb, Camera camera)
        {
            float rangeMultiplier = 1.0f;
            if (camera.Scale.Y > 10000)
            {
                rangeMultiplier = 3.0f;
            }
            else if (camera.Scale.Y > 6500)
            {
                rangeMultiplier = 2.0f;
            }

            if (camera.Scale.Y > 3000)
            {
                foreach (SectorLight sl in sectorLights)
                {
                    if(sl.Major && !landmarkLayerSettings.MajorLightsVisible || !sl.Major && !landmarkLayerSettings.MinorLightsVisible) continue;

                    // skip minor lights at zoom levels below 15000
                    if (!sl.Major && camera.Scale.Y < 15000) continue;

                    // skip if outside draw bounds
                    if (!camera.DrawBounds.Contains(sl.Coordinates)) continue;

                    sl.Draw(sb, camera, rangeMultiplier);
                }
            }
        }

        public class LandmarkLayerSettings : ILayerSettings
        {
            public bool MajorLightsVisible = true;
            public bool MinorLightsVisible = true;
            public bool LandmarksVisible = true;
        }

        
    }
}
