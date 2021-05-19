using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

            foreach (SectorLight.Sector s in Sectors)
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

                    LineRenderer.DrawDashedLine(
                        sb,
                        new[] { screenPos, startAnglePoint },
                        Color.DimGray,
                        new[] { 7f, 3f, 0, 0 },
                        Matrix.Identity);
                    LineRenderer.DrawDashedLine(
                        sb,
                        new[] { screenPos, endAnglePoint },
                        Color.DimGray,
                        new[] { 7f, 3f, 0, 0 },
                        Matrix.Identity);
                }
                else
                {
                    Vector2 orientationAnglePoint = new Vector2(
                        screenPos.X + (float)(Math.Sin(s.Orientation) * (s.Range + 5)),
                        screenPos.Y - (float)(Math.Cos(s.Orientation) * (s.Range + 5))
                    );
                    LineRenderer.DrawDashedLine(
                        sb,
                        new[] { screenPos, orientationAnglePoint },
                        Color.DimGray,
                        new[] { 7f, 3f, 0, 0 },
                        Matrix.Identity);
                }

                Utility.Utility.DrawArc(sb, screenPos, s.Range * rangeMultiplier, 5, s.StartAngle, s.EndAngle,
                    s.Color);
            }
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
