using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Math = System.Math;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace NauticalRenderer.Utility
{
    public static class LineRenderer
    {
        public static void DrawLineStrip(SpriteBatch sb, Vector2[] points, Color color)
        {
            DrawLineStrip(sb, points, color, Matrix.Identity);
        }

        public static void DrawLineStrip(SpriteBatch sb, Vector2[] points, Color color, Matrix viewMatrix)
        {
            Utility.basicEffect.View = viewMatrix;

            VertexPositionColor[] verts = new VertexPositionColor[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                verts[i] = new VertexPositionColor(new Vector3(points[i], 0), color);
            }

            Utility.basicEffect.CurrentTechnique.Passes[0].Apply();
            sb.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, verts, 0, verts.Length - 1);
        }

        public static void DrawLineList(SpriteBatch sb, Vector2[] points, Color color, Matrix viewMatrix)
        {
            VertexPositionColor[] verts = new VertexPositionColor[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                verts[i] = new VertexPositionColor(new Vector3(points[i], 0), color);
            }

            DrawLineList(sb, verts, viewMatrix);
        }

        public static void DrawLineList(SpriteBatch sb, VertexPositionColor[] verts, Matrix viewMatrix)
        {
            Utility.basicEffect.View = viewMatrix;
            Utility.basicEffect.CurrentTechnique.Passes[0].Apply();
            sb.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, verts, 0, verts.Length / 2);
        }

        public static void DrawDashedLine(SpriteBatch sb, Vector2[] points, Color color, float[] lineAndGapLengths, Matrix viewMatrix)
        {
            List<Vector2> drawPoints = new List<Vector2>();

            float segmentLength = lineAndGapLengths.Sum();

            float passedDistance = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector2 startPoint = points[i];
                Vector2 endPoint = points[i + 1];
                Vector2 normal = endPoint - startPoint;
                float length = normal.Length();
                normal.Normalize();

                float internalPassedDistance = passedDistance;
                int segmentIndex = 0;
                while (lineAndGapLengths[segmentIndex] < internalPassedDistance)
                {
                    internalPassedDistance -= lineAndGapLengths[segmentIndex];
                    segmentIndex = ++segmentIndex % lineAndGapLengths.Length;
                }

                float lengthToGo = length;
                float thisSegmentPartLength = lineAndGapLengths[segmentIndex] - internalPassedDistance;
                while (lengthToGo > 0)
                {
                    if (segmentIndex % 2 == 0)
                    {
                        drawPoints.Add(startPoint + normal * (length - lengthToGo));
                        drawPoints.Add(startPoint + normal * (length - lengthToGo + Math.Min(lengthToGo, thisSegmentPartLength)));
                    }

                    lengthToGo -= thisSegmentPartLength;
                    segmentIndex = ++segmentIndex % lineAndGapLengths.Length;
                    thisSegmentPartLength = lineAndGapLengths[segmentIndex];
                }

                passedDistance += length;
            }

            if(drawPoints.Count > 0)
                DrawLineList(sb, drawPoints.ToArray(), color, viewMatrix);
        }

        public static void DrawStyledLine(SpriteBatch sb, Vector2[] points, Color color, LineStyle lineStyle, Matrix viewMatrix)
        {
            List<Vector2> drawPoints = new List<Vector2>();

            float segmentLength = lineStyle.Length;

            float passedDistance = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector2 startPoint = points[i];
                Vector2 endPoint = points[i + 1];
                Vector2 normal = endPoint - startPoint;
                float length = normal.Length();
                normal.Normalize();

                float internalPassedDistance = passedDistance;
                int segmentIndex = 0;
                while (lineStyle.Segments[segmentIndex].length < internalPassedDistance)
                {
                    internalPassedDistance -= lineStyle.Segments[segmentIndex].length;
                    segmentIndex = ++segmentIndex % lineStyle.Segments.Length;
                }

                float lengthToGo = length;
                float thisSegmentPartLength = lineStyle.Segments[segmentIndex].length - internalPassedDistance;
                while (lengthToGo > 0)
                {
                    switch (lineStyle.Segments[segmentIndex].style)
                    {
                        case LineStyle.SegmentStyle.LINE:
                            drawPoints.Add(startPoint + normal * (length - lengthToGo));
                            drawPoints.Add(startPoint + normal * (length - lengthToGo + Math.Min(lengthToGo, thisSegmentPartLength)));
                            break;
                        case LineStyle.SegmentStyle.DOT:
                            drawPoints.Add(startPoint + normal * (length - lengthToGo));
                            drawPoints.Add(startPoint + normal * (length - lengthToGo));
                            break;
                        case LineStyle.SegmentStyle.IMAGE:
                            if (internalPassedDistance <= 0)
                            {
                                sb.Draw(lineStyle.Image,
                                    new RectangleF(startPoint + normal * (length - lengthToGo),
                                        new Vector2(thisSegmentPartLength, lineStyle.Width)),
                                    null,
                                    Color.White,
                                    normal.ToRadiansRotation(),
                                    new Vector2(0, lineStyle.Image.Height / 2f));
                            }
                            break;
                        default:
                            if(lineStyle.Segments[segmentIndex].style != LineStyle.SegmentStyle.SPACE)
                                throw new Exception("Missing rendering code for line segment style.");
                            break;
                    }
                        
                    lengthToGo -= thisSegmentPartLength;
                    segmentIndex = ++segmentIndex % lineStyle.Segments.Length;
                    thisSegmentPartLength = lineStyle.Segments[segmentIndex].length;
                    internalPassedDistance = 0;
                }

                passedDistance += length;
            }

            if (drawPoints.Count > 0)
                DrawLineList(sb, drawPoints.ToArray(), color, viewMatrix);
        }
    }
}
