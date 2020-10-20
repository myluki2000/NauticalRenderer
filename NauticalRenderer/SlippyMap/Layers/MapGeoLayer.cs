using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NauticalRenderer.Data;
using NauticalRenderer.Utility;
using NetTopologySuite.Geometries;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Streams;
using OsmSharp.Streams.Complete;
using OsmSharp.Streams.Filters;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using Point = NetTopologySuite.Geometries.Point;
using Polygon = NetTopologySuite.Geometries.Polygon;

namespace NauticalRenderer.SlippyMap.Layers
{
    class MapGeoLayer : MapLayer
    {
        public List<Vector2[]> Coastlines { get; private set; }
        private List<Vector2[]> breakwaters;
        private List<Vector2[]> piers;
        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        private (bool corner, Vector2 point)[] boundingPolygon;
        private readonly List<Mesh> coastMeshes = new List<Mesh>();
        private List<Mesh> waterMeshes = new List<Mesh>();
        private List<Mesh> innerIslandMeshes = new List<Mesh>();
        private Mesh boundingMesh = new Mesh();


        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            boundingMesh.Draw(mapSb, camera.GetMatrix());

            foreach (Mesh coastMesh in coastMeshes)
            {
                if(coastMesh.BoundingRectangle.Intersects(camera.DrawBounds))
                    coastMesh.Draw(mapSb, camera.GetMatrix());
            }

            foreach (Mesh waterMesh in waterMeshes)
            {
                if (waterMesh.BoundingRectangle.Intersects(camera.DrawBounds))
                    waterMesh.Draw(mapSb, camera.GetMatrix());
            }

            foreach (Mesh mesh in innerIslandMeshes)
            {
                if (mesh.BoundingRectangle.Intersects(camera.DrawBounds))
                    mesh.Draw(mapSb, camera.GetMatrix());
            }

            foreach (Vector2[] line in breakwaters)
            {
                LineRenderer.DrawLineStrip(mapSb, line, Color.DimGray, camera.GetMatrix());
            }

            if (camera.Scale.Y > 3000)
            {
                foreach (Vector2[] line in piers)
                {
                    LineRenderer.DrawLineStrip(mapSb, line, Color.Purple, camera.GetMatrix());
                }
            }
        }

        public override void LoadContent(MapPack mapPack)
        {
            OsmCompleteStreamSource source = new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete();

            IEnumerable<ICompleteOsmGeo> coastlines = from osmGeo in source
                                                      where osmGeo.Type == OsmGeoType.Way && osmGeo.Tags.Contains("natural", "coastline")
                                                      select osmGeo;
            Coastlines = OsmHelpers.WaysToListOfVector2Arr(coastlines);

            IEnumerable<ICompleteOsmGeo> breakwaters = from osmGeo in source
                                                       where osmGeo.Tags.Contains("man_made", "breakwater")
                                                       select osmGeo;
            this.breakwaters = OsmHelpers.WaysToListOfVector2Arr(breakwaters);

            IEnumerable<ICompleteOsmGeo> piers = from osmGeo in source
                                                 where osmGeo.Type == OsmGeoType.Way && osmGeo.Tags.Contains("man_made", "pier")
                                                 select osmGeo;
            this.piers = OsmHelpers.WaysToListOfVector2Arr(piers);

            IEnumerable<ICompleteOsmGeo> waters = from osmGeo in source
                                                  where (osmGeo.Type == OsmGeoType.Way || osmGeo.Type == OsmGeoType.Relation) && osmGeo.Tags.Contains("natural", "water")
                                                  select osmGeo;

            List<Vector2[]> waterWays = new List<Vector2[]>();
            List<Vector2[]> innerIslands = new List<Vector2[]>();
            foreach (ICompleteOsmGeo osmGeo in waters)
            {
                if (osmGeo is CompleteWay way) waterWays.Add(OsmHelpers.WayToVector2Arr(way));
                else if (osmGeo is CompleteRelation relation)
                {

                    List<Vector2[]> relationOuterWays = relation.Members
                        .Where(x => x.Member.Type == OsmGeoType.Way && x.Role == "outer")
                        .Select(x => OsmHelpers.WayToVector2Arr((CompleteWay)x.Member))
                        .ToList();

                    relationOuterWays.RemoveAll(x =>
                    {
                        if (x[0] == x[x.Length - 1])
                        {
                            waterWays.Add(x);
                            return true;
                        }

                        return false;
                    });
                    ConnectWays(relationOuterWays);
                    waterWays.AddRange(relationOuterWays);

                    List<Vector2[]> relationInnerWays = relation.Members
                        .Where(x => x.Member.Type == OsmGeoType.Way && x.Role == "inner")
                        .Select(x => OsmHelpers.WayToVector2Arr((CompleteWay)x.Member))
                        .ToList();

                    relationInnerWays.RemoveAll(x =>
                    {
                        if (x[0] == x[x.Length - 1])
                        {
                            innerIslands.Add(x);
                            return true;
                        }

                        return false;
                    });
                    ConnectWays(relationInnerWays);
                    innerIslands.AddRange(relationInnerWays);
                }

            }


            foreach (Vector2[] waterWay in waterWays)
            {
                if (waterWay[0] == waterWay[waterWay.Length - 1])
                {
                    waterMeshes.Add(new Mesh(Utility.Utility.Triangulate(waterWay), Color.FromNonPremultiplied(58, 147, 214, 255)));
                }
            }

            foreach (Vector2[] innerIsland in innerIslands)
            {
                if (innerIsland[0] == innerIsland[innerIsland.Length - 1])
                {
                    innerIslandMeshes.Add(new Mesh(Utility.Utility.Triangulate(innerIsland), Color.FromNonPremultiplied(213, 203, 161, 255)));
                }
            }

            SplitBoundingPoly();
            GenerateCoastMeshes();
        }

        public void LoadBoundingPoly(Stream boundingPolyStream)
        {
            List<(bool corner, Vector2 point)> coords = new List<(bool corner, Vector2 point)>();
            using (StreamReader sr = new StreamReader(boundingPolyStream))
            {
                // skip first two lines
                sr.ReadLine();
                sr.ReadLine();

                string line = sr.ReadLine();
                while (line != "END")
                {
                    line = line.Trim();
                    line = line.Replace("   ", " ");
                    string[] coordsString = line.Split(' ');

                    coords.Add((true,
                                new Vector2(float.Parse(coordsString[0], CultureInfo.InvariantCulture),
                                            -float.Parse(coordsString[1], CultureInfo.InvariantCulture))));

                    line = sr.ReadLine();
                }
            }

            boundingPolygon = coords.ToArray();
        }

        private void ConnectWays(List<Vector2[]> ways, bool isCoastline = false, bool connectWithBoundingPoly = false)
        {
            for (int i = 0; i < ways.Count; i++)
            {
                // if coastline is closed to itself we can go to the next one
                 if (ways[i][0] == ways[i][ways[i].Length - 1]) continue;

                // check for connection to other coastline pieces
                bool connectionFound = false;
                foreach (Vector2[] otherWay in ways)
                {
                    // if coastline is closed to itself or if same way we can go to the next one
                    if (otherWay[0] == otherWay[otherWay.Length - 1] || ways[i] == otherWay) continue;

                    if (ways[i][ways[i].Length - 1] == otherWay[0])
                    {
                        Vector2[] newArr = new Vector2[ways[i].Length + otherWay.Length - 1];
                        ways[i].CopyTo(newArr, 0);
                        otherWay.CopyTo(newArr, ways[i].Length - 1);
                        ways[i] = newArr;

                        ways.Remove(otherWay);

                        connectionFound = true;

                        // repeat with same index; there might be more lines to connect
                        i--;
                        break;
                    }

                    if (!isCoastline)
                    {
                        if (ways[i][0] == otherWay[otherWay.Length - 1])
                        {
                            Vector2[] newArr = new Vector2[ways[i].Length + otherWay.Length];
                            otherWay.CopyTo(newArr, 0);
                            ways[i].CopyTo(newArr, otherWay.Length);
                            ways[i] = newArr;
                            ways.Remove(otherWay);

                            // repeat with same index; there might be more lines to connect
                            i--;
                            break;
                        }

                        if (ways[i][0] == otherWay[0])
                        {
                            Vector2[] newArr = new Vector2[ways[i].Length + otherWay.Length];
                            otherWay.Reverse().ToArray().CopyTo(newArr, 0);
                            ways[i].CopyTo(newArr, otherWay.Length);
                            ways[i] = newArr;
                            ways.Remove(otherWay);

                            // repeat with same index; there might be more lines to connect
                            i--;
                            break;
                        }

                        if (ways[i][ways[i].Length - 1] == otherWay[otherWay.Length - 1])
                        {
                            Vector2[] newArr = new Vector2[ways[i].Length + otherWay.Length];
                            ways[i].CopyTo(newArr, 0);
                            otherWay.Reverse().ToArray().CopyTo(newArr, ways[i].Length);
                            ways[i] = newArr;
                            ways.Remove(otherWay);

                            // repeat with same index; there might be more lines to connect
                            i--;
                            break;
                        }
                    }
                }

                if (connectWithBoundingPoly && !connectionFound)
                {
                    // check for boundary polygon connections
                    Vector2 endPoint = ways[i][ways[i].Length - 1];
                    int index = Array.FindIndex(boundingPolygon, x => x.point.Equals(endPoint, 0.00001f));
                    if (index != -1)
                    {
                        // coastline point is on bounding poly edge
                        List<Vector2> newCoastSegment = new List<Vector2>();
                        newCoastSegment.AddRange(ways[i]);

                        do
                        {
                            index = (boundingPolygon.Length + index + 1) % boundingPolygon.Length;

                            newCoastSegment.Add(boundingPolygon[index].point);
                        } while (boundingPolygon[index].corner);

                        ways[i] = newCoastSegment.ToArray();

                        // do same segment again if there are more parts to connect
                        i--;
                        continue;
                    }
                }
            }
        }

        private void GenerateCoastMeshes()
        {
            foreach (Vector2[] line in Coastlines)
            {
                if (line.Contains(new Vector2(12.424892f, -54.246006f)))
                {
                    continue;
                }
            }

            Polygon tmpBoundingPoly = new Polygon(new LinearRing(boundingPolygon.Select(x => new Coordinate(x.point.X, x.point.Y)).ToArray()));
            // remove points outside of bounding polygon
            for (int i = 0; i < Coastlines.Count; i++)
            {
                Coastlines[i] = Coastlines[i]
                    .SkipWhile(x => !tmpBoundingPoly.Contains(new Point(x.X, x.Y)) && !tmpBoundingPoly.Coordinates.Contains(new Coordinate(x.X, x.Y)))
                    .ToArray();

                int pointIndex = Array.FindIndex(Coastlines[i],
                    x => !tmpBoundingPoly.Contains(new Point(x.X, x.Y)) && !tmpBoundingPoly.Coordinates.Contains(new Coordinate(x.X, x.Y)));

                if (pointIndex == -1) continue;

                while (pointIndex != -1)
                {
                    Coastlines.Add(Coastlines[i].Take(pointIndex).ToArray());
                    Coastlines[i] = Coastlines[i]
                        .Skip(pointIndex)
                        .SkipWhile(x => !tmpBoundingPoly.Contains(new Point(x.X, x.Y)) && !tmpBoundingPoly.Coordinates.Contains(new Coordinate(x.X, x.Y)))
                        .ToArray();

                    pointIndex = Array.FindIndex(Coastlines[i],
                        x => !tmpBoundingPoly.Contains(new Point(x.X, x.Y)) && !tmpBoundingPoly.Coordinates.Contains(new Coordinate(x.X, x.Y)));
                }
            }

            Coastlines.RemoveAll(x => x.Length < 2);

            foreach (Vector2[] line in Coastlines)
            {
                if (line.Contains(new Vector2(12.424892f, -54.246006f)))
                {
                    continue;
                }
            }

            ConnectWays(Coastlines, true, true);

            
            foreach (Vector2[] coastline in Coastlines)
            {
                if (coastline[0] == coastline[coastline.Length - 1])
                {
                    coastMeshes.Add(new Mesh(Utility.Utility.Triangulate(coastline), Color.FromNonPremultiplied(213, 203, 161, 255)));
                    //coastMeshes.Add(new Mesh(coastline, /*Utility.Utility.TriangulateOld(coastline)*/ new int[] { }, Color.FromNonPremultiplied(213, 203, 161, 255)));
                }
            }
        }

        private void SplitBoundingPoly()
        {
            LinkedList<(bool corner, Vector2 point)> newBoundingPoly = new LinkedList<(bool corner, Vector2 Point)>(boundingPolygon);

            for (int i = 0; i < newBoundingPoly.Count; i++)
            {
                Vector2 p0 = newBoundingPoly.ElementAt(i).point;
                Vector2 p1 = newBoundingPoly.ElementAt((i + 1) % newBoundingPoly.Count).point;

                for (int i2 = 0; i2 < Coastlines.Count; i2++)
                {
                    for (int i3 = 0; i3 < Coastlines[i2].Length - 1; i3++)
                    {
                        Vector2 p2 = Coastlines[i2][i3];
                        Vector2 p3 = Coastlines[i2][i3 + 1];

                        (bool intersecting, Vector2 intersectionPoint) =
                            Utility.Utility.LineLineIntersection(p0, p1, p2, p3);

                        if (intersecting && !newBoundingPoly.Contains((false, intersectionPoint)))
                        {
                            LinkedList<Vector2> newCoastlines = new LinkedList<Vector2>(Coastlines[i2]);
                            newCoastlines.AddAfter(newCoastlines.Find(p2), intersectionPoint);
                            newBoundingPoly.AddAfter(newBoundingPoly.Find((false, p0)) ?? newBoundingPoly.Find((true, p0)), (false, intersectionPoint));
                            Coastlines[i2] = newCoastlines.ToArray();
                            i--;
                            goto end_outer_loop;
                        }
                    }
                }

            end_outer_loop:
                continue;
            }

            boundingPolygon = newBoundingPoly.ToArray();
            Vector2[] boundingPoints = boundingPolygon.Select(x => x.point).ToArray();
            boundingMesh = new Mesh(boundingPoints, Utility.Utility.TriangulateOld(boundingPoints), Color.FromNonPremultiplied(58, 147, 214, 255));
        }
    }
}
