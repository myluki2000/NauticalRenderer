using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Graphics;
using NauticalRenderer.Utility;

namespace NauticalRenderer.SlippyMap.Data
{
    public struct SectorLight
    {
        public Vector2 Coordinates;
        public bool Major;
        public Sector[] Sectors;

        public void Draw(SpriteBatch sb, Camera camera, float rangeMultiplier = 1.0f)
        {
            Vector2 screenPos = Coordinates.Transform(camera.GetMatrix());

            int dashedLineIndex = Sectors.Sum(s => float.IsNaN(s.Orientation) ? 2 : 1);
            Vector2[] dashedLinePoints = new Vector2[dashedLineIndex * 2];
            dashedLineIndex = 0;

            foreach (Sector s in Sectors)
            {
                // draw dashed sector boundaries if light is sector light. If it is directional light draw dashed line in the middle
                if (float.IsNaN(s.Orientation))
                {
                    if (s.StartAngle == s.EndAngle)
                        continue;

                    Vector2 startAnglePoint = new Vector2(
                        screenPos.X + (float)(Math.Sin(s.StartAngle) * (s.Range * rangeMultiplier + 5)),
                        screenPos.Y - (float)(Math.Cos(s.StartAngle) * (s.Range * rangeMultiplier + 5))
                    );

                    Vector2 endAnglePoint = new Vector2(
                        screenPos.X + (float)(Math.Sin(s.EndAngle) * (s.Range * rangeMultiplier + 5)),
                        screenPos.Y - (float)(Math.Cos(s.EndAngle) * (s.Range * rangeMultiplier + 5))
                    );

                    dashedLinePoints[dashedLineIndex++] = screenPos;
                    dashedLinePoints[dashedLineIndex++] = startAnglePoint;
                    dashedLinePoints[dashedLineIndex++] = screenPos;
                    dashedLinePoints[dashedLineIndex++] = endAnglePoint;
                }
                else
                {
                    Vector2 orientationAnglePoint = new Vector2(
                        screenPos.X + (float)(Math.Sin(s.Orientation) * (s.Range + 5)),
                        screenPos.Y - (float)(Math.Cos(s.Orientation) * (s.Range + 5))
                    );

                    dashedLinePoints[dashedLineIndex++] = screenPos;
                    dashedLinePoints[dashedLineIndex++] = orientationAnglePoint;
                }

                Utility.Utility.DrawArc(sb, screenPos, s.Range * rangeMultiplier, 5, s.StartAngle, s.EndAngle,
                    s.Color);
            }

            if(dashedLinePoints.Length > 0)
                LineRenderer.DrawDashedLine(
                sb,
                dashedLinePoints,
                Color.DimGray,
                new[] { 7f, 3f, 0, 0 },
                Matrix.Identity,
                PrimitiveType.LineList);
        }

        public struct Sector
        {
            public float StartAngle;
            public float EndAngle;
            public float Orientation;
            public float Range;
            public Color Color;
        }
    }
}
