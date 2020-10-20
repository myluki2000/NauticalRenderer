using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace NauticalRenderer.Utility
{
    public struct RectangleF
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public float X
        {
            get => Position.X;
            set => Position = new Vector2(value, Position.Y);
        }
        public float Y
        {
            get => Position.Y;
            set => Position = new Vector2(Position.X, value);
        }
        public float Width
        {
            get => Size.X;
            set => Size = new Vector2(value, Size.Y);
        }

        public float Height
        {
            get => Size.Y;
            set => Size = new Vector2(Size.X, value);
        }

        public float Bottom => Y + Height;
        public float Right => X + Width;

        public RectangleF(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        public RectangleF(float x, float y, float width, float height)
        {
            Position = new Vector2();
            Size = new Vector2();
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Contains(Vector2 point)
        {
            return point.X >= Position.X && point.X <= Position.X + Size.X && point.Y >= Position.Y &&
                   point.Y <= Position.Y + Size.Y;
        }

        public bool Intersects(RectangleF value)
        {
            return value.X < X + Width &&
                   X < value.X + value.Width &&
                   value.Y < Y + Height &&
                   Y < value.Y + value.Height;
        }

        public Vector2 Center
        {
            get => new Vector2(X + Width / 2, Y + Height / 2);
        }
    }
}
