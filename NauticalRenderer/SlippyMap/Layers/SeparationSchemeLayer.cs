using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Data;
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
        public static List<Mesh> SeparationZones { get; set; } = new List<Mesh>();
        public static List<Vector2[]> SeparationLanes { get; set; } = new List<Vector2[]>();
        public static List<Vector2[]> SeparationBoundaries { get; set; } = new List<Vector2[]>();
        public static List<Vector2[]> SeparationLines { get; set; } = new List<Vector2[]>();

        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            foreach (Mesh zone in SeparationZones)
            {
                zone.Draw(mapSb, camera.GetMatrix());
            }

            foreach (Vector2[] boundary in SeparationBoundaries)
            {
                LineRenderer.DrawDashedLine(mapSb, boundary, Color.Magenta, new []{0.005f, 0.003f}, camera.GetMatrix());
            }

            foreach (Vector2[] line in SeparationLines)
            {
                LineRenderer.DrawLineStrip(mapSb, line, Color.Magenta, camera.GetMatrix());
            }

            foreach (Vector2[] lane in SeparationLanes)
            {
                for (int i = 0; i < lane.Length - 1; i += 2)
                {
                    Vector2 p1 = lane[i].Transform(camera.GetMatrix());
                    Vector2 p2 = lane[(i + 1) % lane.Length].Transform(camera.GetMatrix());


                    Utility.Utility.DrawBlockArrow(sb, Matrix.Identity, p1, p2, 0.0035f * camera.Scale.Y, Color.Magenta);
                }
            }
        }

        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        public override void LoadContent(MapPack mapPack)
        {
            OsmCompleteStreamSource source = new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete();

            SeparationZones.Clear();
            SeparationLanes.Clear();
            SeparationBoundaries.Clear();

            IEnumerable<ICompleteOsmGeo> separationZones = from osmGeo in source
                                                           where osmGeo.Tags.Contains("seamark:type", "separation_zone")
                                                           select osmGeo;

            foreach (Vector2[] zone in OsmHelpers.WaysToListOfVector2Arr(separationZones))
            {
                SeparationZones.Add(new Mesh(zone, Utility.Utility.TriangulateOld(zone), Color.Magenta));
            }

            IEnumerable<ICompleteOsmGeo> separationBoundaries = from osmGeo in source
                                                                where osmGeo.Tags.Contains("seamark:type", "separation_boundary")
                                                                select osmGeo;

            SeparationBoundaries = OsmHelpers.WaysToListOfVector2Arr(separationBoundaries);

            IEnumerable<ICompleteOsmGeo> separationLanes = from osmGeo in source
                                                           where osmGeo.Tags.Contains("seamark:type", "separation_lane")
                                                           select osmGeo;

            SeparationLanes = OsmHelpers.WaysToListOfVector2Arr(separationLanes);

            IEnumerable<ICompleteOsmGeo> separationLines = from osmGeo in source
                                                           where osmGeo.Tags.Contains("seamark:type", "separation_line")
                                                           select osmGeo;

            SeparationLines = OsmHelpers.WaysToListOfVector2Arr(separationLines);
        }
    }
}
