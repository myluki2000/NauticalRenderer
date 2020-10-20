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

                        throw new Exception("Could not parse landmark category! String was \"" + categoryString + "\"");
                    }

                    return new Landmark(Landmark.LandmarkCategory.GENERIC, OsmHelpers.GetCoordinateOfOsmGeo(x));
                })
                .ToArray();
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

                    bool drawSectorLights = (!minor && camera.Scale.Y > 3000) || camera.Scale.Y > 15000;

                    DrawLight(sb, o, drawSectorLights, camera);

                    // draw name if zoomed in far enough
                    if (camera.Scale.Y > 5000)
                        DrawName(sb, o, camera);
                }
            }
        }

        private void DrawName(SpriteBatch sb, ICompleteOsmGeo o, Camera camera)
        {
            if (!o.Tags.ContainsKey("seamark:name")) return;

            Vector2 pos = OsmHelpers.GetCoordinateOfOsmGeo(o);


            sb.DrawString(Fonts.Arial.Regular,
                o.Tags["seamark:name"],
                pos.Transform(camera.GetMatrix()),
                Color.Black,
                0,
                Vector2.Zero,
                0.8f,
                SpriteEffects.None,
                0f);
        }

        private void DrawLight(SpriteBatch sb, ICompleteOsmGeo o, bool drawSectorLights, Camera camera)
        {
            Color color = Color.DarkViolet; // default color for multi-color lights

            // check if object even has a light
            o.Tags.TryGetValue("seamark:light:colour", out string lightColor);
            if (lightColor != null)
            {
                // handle single color seamark light blob
                color = OsmHelpers.GetColorFromSeamarkColor(lightColor);
            }
            else
            {
                List<Tag> colourKeyValuePairs = o.Tags.Where(x => x.Key.StartsWith("seamark:light:") && x.Key.EndsWith(":colour")).ToList();
                if (colourKeyValuePairs.Count > 0)
                {
                    if (colourKeyValuePairs.Count == 1) color = OsmHelpers.GetColorFromSeamarkColor(colourKeyValuePairs[0].Value);

                    // draw sector lights
                    if (drawSectorLights) DrawSectorLights(sb, o, camera);
                }
                else
                {
                    return; // object doesn't have a light
                }
            }


            Vector2 pos = OsmHelpers.GetCoordinateOfOsmGeo(o).Transform(camera.GetMatrix());

            sb.Draw(Icons.LightBlob, pos, null, color, MathHelper.ToRadians(135), new Vector2(Icons.LightBlob.Width / 2, Icons.LightBlob.Height), new Vector2(0.20f), SpriteEffects.None, 0f);
        }

        private void DrawSectorLights(SpriteBatch sb, ICompleteOsmGeo o, Camera camera)
        {
            int i = 1;
            while (true)
            {
                float startAngle;
                float endAngle;
                float range = 30;
                if (camera.Scale.Y > 10000)
                {
                    range = 80;
                } 
                else if (camera.Scale.Y > 6500)
                {
                    range = 55;
                }

                float orientation = float.NaN;

                // split up the tags of the different lights in separate lists
                Dictionary<string, Tag> tags = o.Tags.Where(x => x.Key.StartsWith("seamark:light:" + i + ":")).ToDictionary(x => x.Key.Replace("seamark:light:" + i + ":", ""));
                i++;

                if (tags.Count == 0) break;

                if (!tags.ContainsKey("colour")) continue;

                if (tags.TryGetValue("range", out Tag rangeTag))
                {
                    range = float.Parse(rangeTag.Value, CultureInfo.InvariantCulture) * (range / 10);
                }

                if (tags.ContainsKey("sector_start") && tags.ContainsKey("sector_end") && tags["sector_start"].Value != tags["sector_end"].Value)
                {
                    startAngle = Utility.Utility.NormalizeAngleRad(
                        MathHelper.ToRadians(float.Parse(tags["sector_start"].Value, CultureInfo.InvariantCulture)) - MathHelper.Pi);
                    endAngle = Utility.Utility.NormalizeAngleRad(
                        MathHelper.ToRadians(float.Parse(tags["sector_end"].Value, CultureInfo.InvariantCulture)) - MathHelper.Pi);
                }
                else
                {
                    // check if light has orientation instead
                    if (!tags.ContainsKey("orientation"))
                    {
                        if (tags.ContainsKey("sector_start") && tags.ContainsKey("sector_end"))
                        {
                            orientation = float.Parse(tags["sector_start"].Value, CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            continue;
                        }

                    }
                    else
                    {
                        orientation = MathHelper.ToRadians(float.Parse(tags["orientation"].Value, CultureInfo.InvariantCulture)) - MathHelper.Pi;
                    }


                    startAngle = Utility.Utility.NormalizeAngleRad(orientation - 0.05f);
                    endAngle = Utility.Utility.NormalizeAngleRad(orientation + 0.05f);
                }

                Vector2 pos = OsmHelpers.GetCoordinateOfOsmGeo(o).Transform(camera.GetMatrix());

                Color color = OsmHelpers.GetColorFromSeamarkColor(tags["colour"].Value);

                // draw dashed sector boundaries if light is sector light. If it is directional light draw dashed line in the middle
                if (float.IsNaN(orientation))
                {
                    if (startAngle == endAngle)
                        continue;

                    Vector2 startAnglePoint = new Vector2(
                        pos.X + (float)(Math.Sin(startAngle) * (range + 5)),
                        pos.Y - (float)(Math.Cos(startAngle) * (range + 5))
                    );

                    Vector2 endAnglePoint = new Vector2(
                        pos.X + (float)(Math.Sin(endAngle) * (range + 5)),
                        pos.Y - (float)(Math.Cos(endAngle) * (range + 5))
                    );

                    LineRenderer.DrawDashedLine(sb, new[] { pos, startAnglePoint }, Color.DimGray, new[] { 7f, 3f }, Matrix.Identity);
                    LineRenderer.DrawDashedLine(sb, new[] { pos, endAnglePoint }, Color.DimGray, new[] { 7f, 3f }, Matrix.Identity);
                }
                else
                {
                    Vector2 orientationAnglePoint = new Vector2(
                        pos.X + (float)(Math.Sin(orientation) * (range + 5)),
                        pos.Y - (float)(Math.Cos(orientation) * (range + 5))
                    );
                    LineRenderer.DrawDashedLine(sb, new[] { pos, orientationAnglePoint }, Color.DimGray, new[] { 7f, 3f }, Matrix.Identity);
                }

                Utility.Utility.DrawArc(sb, pos, range, 5, startAngle, endAngle, color);
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
