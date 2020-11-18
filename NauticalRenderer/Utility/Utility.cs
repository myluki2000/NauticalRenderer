using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Data;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using Point = NetTopologySuite.Geometries.Point;
using Polygon = NetTopologySuite.Geometries.Polygon;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace NauticalRenderer.Utility
{
    public static class Utility
    {
        private static readonly Texture2D dummyTexture = new Texture2D(Globals.Graphics.GraphicsDevice, 1, 1);
        public static readonly BasicEffect basicEffect = new BasicEffect(Globals.Graphics.GraphicsDevice);

        static Utility()
        {
            dummyTexture.SetData(new Color[] { Color.White });

            basicEffect.VertexColorEnabled = true;
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0,
                Globals.Graphics.GraphicsDevice.Viewport.Width,
                Globals.Graphics.GraphicsDevice.Viewport.Height,
                0,
                0,
                1
            );
        }

        public static void DrawArc(SpriteBatch sb,
                                   Vector2 position,
                                   float innerRadius,
                                   float thickness,
                                   float startAngle,
                                   float endAngle,
                                   Color color,
                                   Matrix? viewMatrix = null,
                                   int resolution = 20)
        {
            if (viewMatrix == null) viewMatrix = Matrix.Identity;
            basicEffect.View = (Matrix)viewMatrix;

            if (startAngle > endAngle)
            {
                startAngle = -(MathHelper.TwoPi - startAngle);
            }

            Vector3 innerPos;
            Vector3 outerPos;

            List<VertexPositionColor> verts = new List<VertexPositionColor>(resolution * 3);
            float stepSize = (endAngle - startAngle) / resolution;
            
            for (float angle = startAngle; angle <= endAngle; angle += stepSize)
            {
                innerPos = new Vector3(
                    position.X + (float)(Math.Sin(angle) * innerRadius),
                    position.Y - (float)(Math.Cos(angle) * innerRadius),
                    0
                );

                verts.Add(new VertexPositionColor(innerPos, color));

                outerPos = new Vector3(
                    position.X + (float)(Math.Sin(angle) * (innerRadius + thickness)),
                    position.Y - (float)(Math.Cos(angle) * (innerRadius + thickness)),
                    0
                );

                verts.Add(new VertexPositionColor(outerPos, color));
            }

            innerPos = new Vector3(
                position.X + (float)(Math.Sin(endAngle) * innerRadius),
                position.Y - (float)(Math.Cos(endAngle) * innerRadius),
                0
            );

            verts.Add(new VertexPositionColor(innerPos, color));

            outerPos = new Vector3(
                position.X + (float)(Math.Sin(endAngle) * (innerRadius + thickness)),
                position.Y - (float)(Math.Cos(endAngle) * (innerRadius + thickness)),
                0
            );

            verts.Add(new VertexPositionColor(outerPos, color));

            basicEffect.CurrentTechnique.Passes[0].Apply();
            sb.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts.ToArray(), 0, verts.Count - 2);
        }

        

        public static void DrawRectangle(SpriteBatch sb, Rectangle rect, Color color)
        {
            sb.Draw(dummyTexture, rect, color);
        }

        public static void DrawRectangleF(SpriteBatch sb, RectangleF rect, Color color)
        {
            sb.Draw(dummyTexture, rect, color: color);
        }

        public static void DrawOutline(SpriteBatch sb, Rectangle rect, Color color)
        {
            LineRenderer.DrawLineStrip(sb, new[] { rect.TopLeft().ToVector2(), rect.TopRight().ToVector2(), rect.BottomRight().ToVector2(), rect.BottomLeft().ToVector2(), rect.TopLeft().ToVector2() }, color);
        }

        /// <summary>
        /// Returns an array which contains indices of all elements in the verts list. Basically just 0,1,2,3,4,...,(n-1) if n = verts.Count
        /// </summary>
        /// <param name="verts"></param>
        /// <returns></returns>
        public static int[] GetIndicesArray<T>(List<T> verts)
        {
            int[] result = new int[verts.Count];

            for (int i = 0; i < verts.Count; i++)
            {
                result[i] = i;
            }

            return result;
        }

        /// <summary>
        /// Returns the distance in meters between to geographic coordinates.
        /// </summary>
        /// <returns></returns>
        public static float DistanceBetweenCoordinates(Vector2 pos1, Vector2 pos2)
        {
            int earthRadiusKm = 6371;
            float dLat = (float)Degrees.ToRadians(pos2.Y - pos1.Y);
            float dLon = (float)Degrees.ToRadians(pos2.X - pos1.X);

            float lat1 = (float)Degrees.ToRadians(pos1.Y);
            float lat2 = (float)Degrees.ToRadians(pos2.Y);

            float a = (float)(Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                      Math.Pow(Math.Sin(dLon / 2) * Math.Cos(lat2), 2));
            float c = (float)(2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
            return earthRadiusKm * c * 1000;
        }

        public static float NormalizeAngleRad(float angle)
        {
            angle %= MathHelper.TwoPi;
            if (angle < 0) angle += MathHelper.TwoPi;
            return angle;
        }

        public static Vector2[] Triangulate(Vector2[] points)
        {
            TriangleNet.Geometry.Polygon p = new TriangleNet.Geometry.Polygon(points.Length - 1);
            p.Add(new Contour(points.Take(points.Length - 1).Select(x => new Vertex(x.X, x.Y))));
            return p.Triangulate(new QualityOptions() {MinimumAngle = 25}).Triangles.SelectMany(x => new[] { x.GetVertex(0), x.GetVertex(1), x.GetVertex(2) }).Select(x => new Vector2((float)x.X, (float)x.Y)).ToArray();

        }

        // old triangulation method with rounding errors
        public static int[] TriangulateOld(Vector2[] inVerts)
        {
            List<int> indices = new List<int>();

            double Sign(Vector2 p1, Vector2 p2, Vector2 p3)
            {
                return ((double)p1.X - p3.X) * ((double)p2.Y - p3.Y) - ((double)p2.X - p3.X) * ((double)p1.Y - p3.Y);
            }

            bool PointInTriangle(Vector2 p, Vector2 v1, Vector2 v2, Vector2 v3)
            {
                double d1 = Sign(p, v1, v2);
                double d2 = Sign(p, v2, v3);
                double d3 = Sign(p, v3, v1);

                bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
                bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
                return !(hasNeg && hasPos);
            }

            List<Vector2> verts = new List<Vector2>(inVerts);

            double edgeSum = 0;
            for (int i = 0; i < verts.Count; i++)
            {
                Vector2 p1 = verts[i];
                Vector2 p2 = verts[(i + 1) % verts.Count];

                edgeSum += (p2.X - p1.X) * (p2.Y + p1.Y);
            }

            if (edgeSum >= 0)
            {
                // polygon is clockwise, so we have to reverse the points
                verts.Reverse();
            }

            while (verts.Count > 3)
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    Vector2 p1 = verts[i];
                    Vector2 p2 = verts[(i + 1) % verts.Count];
                    Vector2 p3 = verts[(i + 2) % verts.Count];

                    // if corner is convex
                    double s = Sign(p1, p2, p3);
                    if (s < -0.0000001 || verts.Exists(x => x != p1 && x != p2 && x != p3 && PointInTriangle(x, p1, p2, p3)))
                    {
                        continue;
                    }

                    indices.Add(Array.IndexOf(inVerts, p1));
                    indices.Add(Array.IndexOf(inVerts, p2));
                    indices.Add(Array.IndexOf(inVerts, p3));

                    verts.Remove(p2);

                    break;
                }
            }

            indices.Add(Array.IndexOf(inVerts, verts[0]));
            indices.Add(Array.IndexOf(inVerts, verts[1]));
            indices.Add(Array.IndexOf(inVerts, verts[2]));

            return indices.ToArray();
        }

        public static void DrawBlockArrow(SpriteBatch sb,
                                          Matrix viewMatrix,
                                          Vector2 startPoint,
                                          Vector2 endPoint,
                                          float thickness,
                                          Color color)
        {
            float baseThickness = thickness / 2.5f;
            Vector2 lengthToBase = endPoint - startPoint;
            Vector2 unitNormal = new Vector2(-lengthToBase.Y, lengthToBase.X);
            lengthToBase *= 0.65f;
            unitNormal.Normalize();

            Vector2[] points = new Vector2[8];
            points[0] = startPoint - unitNormal * baseThickness;
            points[1] = startPoint + unitNormal * baseThickness;
            points[2] = points[1] + lengthToBase;
            points[3] = startPoint + unitNormal * thickness + lengthToBase;
            points[4] = endPoint;
            points[5] = startPoint - unitNormal * thickness + lengthToBase;
            points[6] = points[0] + lengthToBase;
            points[7] = points[0];

            LineRenderer.DrawLineStrip(sb, points, color, viewMatrix);
        }

        public static (bool intersect, Vector2 point) LineLineIntersection(Vector2 p0,
                                                                           Vector2 p1,
                                                                           Vector2 p2,
                                                                           Vector2 p3,
                                                                           bool includeL1Ends = true)
        {
            Vector2 intersectionPoint = new Vector2();

            double s1X = p1.X - p0.X;
            double s1Y = p1.Y - p0.Y;
            double s2X = p3.X - p2.X;
            double s2Y = p3.Y - p2.Y;

            double s = (-s1Y * (p0.X - p2.X) + s1X * (p0.Y - p2.Y)) / (-s2X * s1Y + s1X * s2Y);
            double t = (s2X * (p0.Y - p2.Y) - s2Y * (p0.X - p2.X)) / (-s2X * s1Y + s1X * s2Y);

            if (includeL1Ends)
            {
                if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
                {
                    // Collision detected
                    intersectionPoint.X = (float)(p0.X + (t * s1X));
                    intersectionPoint.Y = (float)(p0.Y + (t * s1Y));
                    return (true, intersectionPoint);
                }
            }
            else
            {
                if (s > 0 && s < 1 && t >= 0 && t <= 1)
                {
                    // Collision detected
                    intersectionPoint.X = (float)(p0.X + (t * s1X));
                    intersectionPoint.Y = (float)(p0.Y + (t * s1Y));
                    return (true, intersectionPoint);
                }
            }

            return (false, intersectionPoint); // No collision
        }

        /// <summary>
        /// Returns an intersection point between a line and a polyline.
        /// </summary>
        public static (bool intersect, Vector2 point) LinePolylineIntersection(Vector2 p0, Vector2 p1, Vector2[] polyline)
        {
            for (int i = 0; i < polyline.Length; i++)
            {
                Vector2 p2 = polyline[i];
                Vector2 p3 = polyline[(i + 1) % polyline.Length];

                (bool intersect, Vector2 point) = LineLineIntersection(p0, p1, p2, p3, false);
                if (intersect) return (intersect, point);
            }

            return (false, Vector2.Zero);
        }

        public static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
        {
            Polygon p = new Polygon(new LinearRing(polygon.Select(x => new Coordinate(x.X, x.Y)).ToArray()));
            return p.Contains(new Point(point.X, point.Y));

            // old code that sometimes outputs incorrect results (rounding error?)
            /*ushort count = 0;
            const double s1X = 1;
            const double s1Y = 0;
            for (int i = 0; i < polygon.Length; i++)
            {
                Vector2 p2 = polygon[i];
                Vector2 p3 = polygon[(i + 1) % polygon.Length];

                double s2X = p3.X - p2.X;
                double s2Y = p3.Y - p2.Y;

                double s = (-s1Y * (point.X - p2.X) + s1X * (point.Y - p2.Y)) / (-s2X * s1Y + s1X * s2Y);
                double t = (s2X * (point.Y - p2.Y) - s2Y * (point.X - p2.X)) / (-s2X * s1Y + s1X * s2Y);

                if (s >= 0 && s <= 1 && t >= 0)
                {
                    // Collision detected
                    count++;
                }
            }

            return count % 2 == 1;*/
        }

        public static string WrapText(SpriteFont spriteFont, string text, float maxLineWidth)
        {
            string[] words = text.Split(' ');
            StringBuilder sb = new StringBuilder();
            float lineWidth = 0f;
            float spaceWidth = spriteFont.MeasureString(" ").X;

            foreach (string word in words)
            {
                Vector2 size = spriteFont.MeasureString(word);

                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            return sb.ToString();
        }

        public static Vector2[] LineStripsToLineList(Vector2[][] strips)
        {
            List<Vector2> ll = new List<Vector2>();
            foreach (Vector2[] strip in strips)
            {
                ll.Add(strip[0]);
                for (int i = 1; i < strip.Length - 1; i++)
                {
                    ll.Add(strip[i]);
                    ll.Add(strip[i]);
                }
                ll.Add(strip[^1]);
            }

            return ll.ToArray();
        }
    }
}
