using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using NauticalRenderer.Data;
using NauticalRenderer.Data.MapPack;
using NauticalRenderer.SlippyMap;
using NauticalRenderer.SlippyMap.Layers;
using NauticalRenderer.Utility;
using OsmSharp;
using OsmSharp.API;
using OsmSharp.Complete;
using OsmSharp.Streams;
using OsmSharp.Streams.Complete;

namespace NauticalRenderer.SlippyMap.Layers
{
    public class SeparationSchemeLayer : MapLayer
    {
        private Mesh[] separationZones;
        private Vector2[][] separationLanes;
        private Vector2[][] separationBoundaries;
        private Vector2[][] separationLines;
        private List<(Vector2, string)> labels = new List<(Vector2, string)>();

        private static readonly Color SEPARATION_SCHEME_COLOR = Color.Magenta * 0.5f;
        private static readonly Color SEPARATION_SCHEME_TEXT_COLOR = new Color(96, 0, 96);

        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            foreach (Mesh zone in separationZones)
            {
                zone.Draw(mapSb, camera.GetMatrix());
            }

            foreach (Vector2[] boundary in separationBoundaries)
            {
                LineRenderer.DrawDashedLine(
                    mapSb,
                    boundary,
                    SEPARATION_SCHEME_COLOR,
                    new []{ 6.25f, 3.75f, 0, 0 },
                    camera.GetMatrix());
            }

            foreach (Vector2[] line in separationLines)
            {
                LineRenderer.DrawLineStrip(mapSb, line, SEPARATION_SCHEME_COLOR, camera.GetMatrix());
            }

            foreach (Vector2[] lane in separationLanes)
            {
                for (int i = 0; i < lane.Length - 1; i += 2)
                {
                    Vector2 p1 = lane[i].Transform(camera.GetMatrix());
                    Vector2 p2 = lane[(i + 1) % lane.Length].Transform(camera.GetMatrix());


                    Utility.Utility.DrawBlockArrow(sb, Matrix.Identity, p1, p2, 0.0035f * camera.Scale.Y, SEPARATION_SCHEME_COLOR);
                }
            }

            if(camera.Scale.Y > 2500)
                foreach ((Vector2 coords, string text) in labels)
                {
                    if (camera.DrawBounds.Contains(coords))
                    {
                        Vector2 size = DefaultAssets.FontSmall.MeasureString(text);
                        sb.DrawString(DefaultAssets.FontSmall, text, (coords.Transform(camera.GetMatrix()) - size / 2).Rounded(), SEPARATION_SCHEME_TEXT_COLOR);
                    }
                        
                }
        }

        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        public override void LoadContent(MapPack mapPack)
        {
            OsmCompleteStreamSource source = new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete();

            labels.Clear();


            ILookup<byte, CompleteWay> geos = source
                .OfType<CompleteWay>()
                .Where(x =>
                {
                    if (!x.Tags.TryGetValue("seamark:type", out string value)) return false;

                    if (value != "separation_zone" && value != "separation_boundary" && value != "separation_lane" &&
                        value != "separation_line") return false;

                    if(x.Tags.TryGetValue("seamark:name", out string name))
                        labels.Add((OsmHelpers.GetCoordinateOfOsmGeo(x), name));

                    return true;
                }).ToLookup(x =>
                {
                    return x.Tags["seamark:type"] switch
                    {
                        "separation_zone" => (byte)0,
                        "separation_boundary" => (byte)1,
                        "separation_lane" => (byte)2,
                        "separation_line" => (byte)3,
                    };
                });

            separationZones = geos[0]
                .Select(x => new Mesh(Utility.Utility.Triangulate(OsmHelpers.WayToLineStrip(x)), SEPARATION_SCHEME_COLOR))
                .ToArray();

            separationBoundaries = geos[1]
                .Select(x => OsmHelpers.WayToLineStrip(x))
                .ToArray();

            separationLanes = geos[2]
                .Select(x => OsmHelpers.WayToLineStrip(x))
                .ToArray();

            separationLines = geos[3]
                .Select(x => OsmHelpers.WayToLineStrip(x))
                .ToArray();

        }
    }
}
