using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Utility;

namespace NauticalRenderer.SlippyMap.UI
{
    class MapLabel
    {
        public SpriteFont Font { get; set; }
        public string Text { get; set; }
        public Vector2 Position { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <inheritdoc />
        public MapLabel(string text, Vector2 position, VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment, SpriteFont font)
        {
            Text = text;
            Position = position;
            VerticalAlignment = verticalAlignment;
            HorizontalAlignment = horizontalAlignment;
            Font = font;
        }

        public void Draw(SpriteBatch sb, Color color, Camera camera)
        {
            Vector2 origin = new Vector2(0, 0);
            origin.X = HorizontalAlignment switch
            {
                HorizontalAlignment.LEFT => 0,
                HorizontalAlignment.CENTER => Font.MeasureString(Text).X / 2,
                HorizontalAlignment.RIGHT => Font.MeasureString(Text).X,
                _ => throw new Exception("Unsupported label text alignment")
            };

            origin.Y = VerticalAlignment switch
            {
                VerticalAlignment.TOP => 0,
                VerticalAlignment.MIDDLE => Font.MeasureString(Text).Y / 2,
                VerticalAlignment.BOTTOM => Font.MeasureString(Text).Y,
                _ => throw new Exception("Unsupported label text alignment")
            };

            sb.DrawString(Font, Text, Position, color, 0, origin, Vector2.One, SpriteEffects.None, 0);
        }
    }
}
