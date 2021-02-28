using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.SlippyMap;
using NauticalRenderer.Utility;
using OsmSharp.API;

namespace NauticalRenderer.SlippyMap.UI
{
    class LineText
    {
        public RectangleF BoundingRect { get; }
        private readonly string text;
        private readonly Vector2[] points;
        private readonly SpriteFont font;
        private readonly Alignment alignment;

        public LineText(Vector2[] points, string text, SpriteFont font, Alignment alignment)
        {
            this.font = font;
            this.alignment = alignment;
            this.text = text;
            this.points = points;

            BoundingRect = OsmHelpers.GetBoundingRectOfPoints(points);
        }

        public void Draw(SpriteBatch sb, Color color, Camera camera)
        {
            if (!BoundingRect.Intersects(camera.DrawBounds)) return;

                List<TextSegment> segments = new List<TextSegment>();
            Vector2[] tPoints = new Vector2[points.Length];

            for (int i = 0; i < tPoints.Length; i++)
            {
                tPoints[i] = points[i].Transform(camera.GetMatrix());
            }

            float textLength = font.MeasureString(text).X * 0.00001f;
            float lineLength = Utility.Utility.LengthOfLine(tPoints);

            float startLength = alignment switch
            {
                Alignment.START => 0,
                Alignment.CENTER => lineLength / 2 - textLength / 2,
                Alignment.END => lineLength - textLength,
                _ => throw new Exception("Cannot handle alignment " + alignment),
            };

            float remainingStartLength = startLength;
            string remainingText = text;

            // line is "the wrong way", text would be upside down
            if (Math.Abs((tPoints[1] - tPoints[0]).Direction()) > MathHelper.PiOver2)
            {
                tPoints = tPoints.Reverse().ToArray();
            }

            for (int i = 0; i < tPoints.Length - 1; i++)
            {
                Vector2 p1 = tPoints[i];
                Vector2 p2 = tPoints[i + 1];
                float segmentLength = (p2 - p1).Length();
                if (remainingStartLength >= segmentLength)
                {
                    remainingStartLength -= segmentLength;
                    continue;
                }
                else
                {
                    Vector2 unitDirectionV = (p2 - p1);
                    unitDirectionV.Normalize();

                    Vector2 startPoint = p1 + unitDirectionV * remainingStartLength;

                    float usableLength = (p2 - startPoint).Length();

                    string segmentText = remainingText;
                    while (font.MeasureString(segmentText).X > usableLength + (font.MeasureString("m").X / 4))
                        segmentText = segmentText.Remove(segmentText.Length - 1);

                    remainingText = remainingText.Remove(0, segmentText.Length);

                    segments.Add(new TextSegment()
                    {
                        Position = startPoint,
                        Rotation = unitDirectionV.Direction(),
                        Text = segmentText
                    });

                    remainingStartLength = font.MeasureString(segmentText).X - usableLength;
                }
            }

            if (remainingText == "")
            {
                foreach (TextSegment ts in segments)
                {
                    sb.DrawString(font, ts.Text, ts.Position.Rounded(), color, ts.Rotation,
                        new Vector2(0, font.MeasureString(ts.Text).Y / 2), Vector2.One, SpriteEffects.None, 0);
                }
            }
        }

        private struct TextSegment
        {
            public string Text;
            public Vector2 Position;
            public float Rotation;
        }

        public enum Alignment
        {
            START,
            CENTER,
            END,
        }
    }
}
