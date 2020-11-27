using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using OsmSharp;
using OsmSharp.Complete;

namespace NauticalRenderer.Utility
{
    class OsmHelpers
    {
        public static List<Vector2[]> WaysToListOfVector2Arr(IEnumerable<ICompleteOsmGeo> geo)
        {
            List<Vector2[]> list = new List<Vector2[]>();
            foreach (CompleteWay way in geo)
            {
                list.Add(WayToVector2Arr(way));
            }

            return list;
        }

        public static Vector2[] WayToVector2Arr(CompleteWay way)
        {
            return way.Nodes.Select(x => new Vector2((float) x.Longitude, -(float) x.Latitude)).ToArray();
        }

        public static Color GetColorFromSeamarkColor(string color)
        {
            switch (color.ToLower())
            {
                case "white":
                    return Color.White;
                case "black":
                    return Color.Black;
                case "red":
                    return Color.Red;
                case "green":
                    return Color.Green;
                case "blue":
                    return Color.Blue;
                case "yellow":
                    return Color.Yellow;
                case "grey":
                    return Color.Gray;
                case "brown":
                    return Color.Brown;
                case "amber":
                    return new Color(255, 191, 0);
                case "violet":
                    return Color.Violet;
                case "orange":
                    return Color.Orange;
                case "magenta":
                    return Color.Magenta;
                case "pink":
                    return Color.Pink;
                default:
                    return Color.DarkMagenta;
            }
        }

        public static Vector2 GetCoordinateOfOsmGeo(ICompleteOsmGeo o)
        {
            switch (o)
            {
                case Node n:
                    return new Vector2((float) n.Longitude, -(float) n.Latitude);
                case CompleteWay w:
                    return new Vector2((float)w.Nodes.Select(x => x.Longitude).Average().Value, -(float)w.Nodes.Select(x => x.Latitude).Average().Value);
                case CompleteRelation r:
                    Vector2 sum = Vector2.Zero;
                    foreach (CompleteRelationMember rm in r.Members)
                    {
                        sum += GetCoordinateOfOsmGeo(rm.Member);
                    }
                    return sum / r.Members.Length;
                default:
                    throw new Exception("Encountered unknown osm geo type!");
            }
        }

        public static RectangleF GetBoundingRectOfPoints(Vector2[] points)
        {
            float xMin = float.MaxValue, xMax = float.MinValue, yMin = float.MaxValue, yMax = float.MinValue;
            foreach ((float x, float y) in points)
            {
                if (x < xMin) xMin = x;
                if (x > xMax) xMax = x;
                if (y < yMin) yMin = y;
                if (y > yMax) yMax = y;
            }
            return new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin);
        }
    }
}
