using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using NauticalRenderer.Data;
using NauticalRenderer.Utility;

namespace NauticalRenderer.SlippyMap.Data
{
    struct RestrictedArea
    {
        public string Label { get; }
        public RestrictedAreaCategory Category { get; }
        public RestrictedAreaRestriction Restriction { get; }
        public RectangleF BoundingRectangle { get; }


        public bool IsArea { get; }
        private Mesh mesh;
        private readonly Vector2[] points;

        /// <inheritdoc />
        public RestrictedArea(string label, Vector2[] points, RestrictedAreaCategory category, RestrictedAreaRestriction restriction) : this()
        {
            this.Label = label;
            this.points = points;
            IsArea = points[0] == points[points.Length - 1];
            if (IsArea)
            {
                mesh = new Mesh(points, Utility.Utility.TriangulateOld(points));
                switch (restriction)
                {
                    case RestrictedAreaRestriction.NO_ENTRY:
                        mesh.Color = Color.Red * 0.25f;
                        break;
                }
            }

            float xMin = float.MaxValue, xMax = float.MinValue, yMin = float.MaxValue, yMax = float.MinValue;
            foreach ((float x, float y) in points)
            {
                if (x < xMin) xMin = x;
                if (x > xMax) xMax = x;
                if (y < yMin) yMin = y;
                if (y > yMax) yMax = y;
            }
            BoundingRectangle = new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin);

            Category = category;
            Restriction = restriction;
        }

        public void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            switch (Restriction)
            {
                case RestrictedAreaRestriction.NO_ENTRY:
                    if(IsArea) mesh.Draw(mapSb, camera.GetMatrix());
                    LineRenderer.DrawLineStrip(mapSb, points, Color.Red, camera.GetMatrix());

                    break;
                case RestrictedAreaRestriction.RESTRICTED_ENTRY:
                    LineRenderer.DrawDashedLine(mapSb, points, Color.Black, new [] {0.004f, 0.002f}, camera.GetMatrix());

                    break;
                case RestrictedAreaRestriction.RESTRICTED_FISHING:
                    if (camera.Scale.Y < 5000) return;
                    LineRenderer.DrawStyledLine(mapSb, points, Color.Black, LineStyle.FishingRestricted, camera.GetMatrix());
                    break;
                case RestrictedAreaRestriction.NO_FISHING:
                    if (camera.Scale.Y < 5000) return;
                    LineRenderer.DrawStyledLine(mapSb, points, Color.Black, LineStyle.NoFishing, camera.GetMatrix());
                    break;
                case RestrictedAreaRestriction.NO_ANCHORING:
                    if (camera.Scale.Y < 5000) return;
                    LineRenderer.DrawDashedLine(mapSb, points, Color.Black, new []{0.0005f, 0.001f}, camera.GetMatrix());
                    if (camera.Scale.Y < 15000) return;
                    sb.Draw(Icons.NoAnchoring,
                        BoundingRectangle.Center.Transform(camera.GetMatrix()),
                        null,
                        Color.White,
                        0,
                        new Vector2(Icons.NoAnchoring.Width / 2f, Icons.NoAnchoring.Height / 2f),
                        0.05f,
                        SpriteEffects.None,
                        0);
                    break;
            }

            // TODO: we shouldn't really need to calculate this separately for every area
            Vector2 mouseWorldPos = camera.ScreenPosToWorldPos(Mouse.GetState().Position.ToVector2());

            // TODO: Also allow hovering over lines
            if (points[0] == points[points.Length - 1]
                && BoundingRectangle.Contains(mouseWorldPos)
                && Utility.Utility.IsPointInPolygon(mouseWorldPos, points))
            {
                SpriteFont font = DefaultAssets.FontSmall;
                string formattedLabel = Utility.Utility.WrapText(font, Label, 100);
                Vector2 labelSize = font.MeasureString(formattedLabel);
                sb.DrawString(font, formattedLabel, BoundingRectangle.Center.Transform(camera.GetMatrix()), Color.Black, 0, labelSize / 2, Vector2.One, SpriteEffects.None, 0);
            }
        }

        public enum RestrictedAreaCategory
        {
            UNKNOWN,
            SAFETY,
            NATURE_RESERVE,
            SWIMMING,
            WAITING,
            MILITARY,
        }

        public enum RestrictedAreaRestriction
        {
            UNKNOWN,
            NO_FISHING,
            RESTRICTED_FISHING,
            NO_ENTRY,
            RESTRICTED_ENTRY,
            NO_ANCHORING_NO_FISHING,
            NO_ANCHORING
        }
    }
}
