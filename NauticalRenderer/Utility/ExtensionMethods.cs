using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Xml.Linq;
using GeoAPI.Geometries;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OsmSharp.Tags;

namespace NauticalRenderer.Utility
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Maps this float value in a range to a float value in another range.
        /// </summary>
        public static float Map(this float value, float inputFrom, float inputTo, float outputFrom, float outputTo)
        {
            return (value - inputFrom) / (inputTo - inputFrom) * (outputTo - outputFrom) + outputFrom;
        }

        /// <summary>
        /// Maps this sbyte value in a range to a float value in another range.
        /// </summary>
        public static float Map(this sbyte value, float inputFrom, float inputTo, float outputFrom, float outputTo)
        {
            return (value - inputFrom) / (inputTo - inputFrom) * (outputTo - outputFrom) + outputFrom;
        }

        /// <summary>
        /// Transforms this Vector2 using the specified transformation matrix.
        /// </summary>
        public static Vector2 Transform(this Vector2 value, Matrix transformationMatrix)
        {
            return Vector2.Transform(value, transformationMatrix);
        }

        /// <summary>
        /// Transforms this Vector3 using the specified transformation matrix.
        /// </summary>
        public static Vector3 Transform(this Vector3 value, Matrix transformationMatrix)
        {
            return Vector3.Transform(value, transformationMatrix);
        }

        public static Matrix Invert(this Matrix value)
        {
            return Matrix.Invert(value);
        }

        public static XElement ToXml(this Vector2 value, string name)
        {
            return new XElement(name,
                new XAttribute("X", value.X.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("Y", value.Y.ToString(CultureInfo.InvariantCulture)));
        }

        public static Vector2 FromXml(this Vector2 value, XElement xEle)
        {
            value.X = Single.Parse(xEle.Attribute("X").Value, CultureInfo.InvariantCulture.NumberFormat);
            value.Y = Single.Parse(xEle.Attribute("Y").Value, CultureInfo.InvariantCulture.NumberFormat);

            return value;
        }

        public static XElement ToXml(this Rectangle value, string name)
        {
            return new XElement(name,
                new XAttribute("X", value.X.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("Y", value.Y.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("W", value.Width.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("H", value.Height.ToString(CultureInfo.InvariantCulture)));
        }

        public static Rectangle FromXml(this Rectangle value, XElement xEle)
        {
            value.X = Int32.Parse(xEle.Attribute("X").Value, CultureInfo.InvariantCulture.NumberFormat);
            value.Y = Int32.Parse(xEle.Attribute("Y").Value, CultureInfo.InvariantCulture.NumberFormat);
            value.Width = Int32.Parse(xEle.Attribute("W").Value, CultureInfo.InvariantCulture.NumberFormat);
            value.Height = Int32.Parse(xEle.Attribute("H").Value, CultureInfo.InvariantCulture.NumberFormat);

            return value;
        }

        public static Point TopLeft(this Rectangle value)
        {
            return value.Location;
        }

        public static Point TopRight(this Rectangle value)
        {
            return new Point(value.Right, value.Top);
        }

        public static Point BottomLeft(this Rectangle value)
        {
            return new Point(value.Left, value.Bottom);
        }

        public static Point BottomRight(this Rectangle value)
        {
            return new Point(value.Right, value.Bottom);
        }

        /// <summary>
        /// Boolean that indicates if mouse position is inside the game window.
        /// </summary>
        /// <param name="value">This mouse state.</param>
        /// <returns>Returns true if mouse is in game window, false otherwise.</returns>
        public static bool IsInWindow(this MouseState value)
        {
            return value.X >= 0 && value.X < Globals.Graphics.GraphicsDevice.Viewport.Width
                                && value.Y >= 0 && value.Y < Globals.Graphics.GraphicsDevice.Viewport.Height;
        }

        public static Rectangle Transform(this Rectangle value, Matrix matrix)
        {
            Vector2 topLeft = value.TopLeft().ToVector2().Transform(matrix);
            Vector2 bottomRight = value.BottomRight().ToVector2().Transform(matrix);
            return new Rectangle(topLeft.ToPoint(), (bottomRight - topLeft).ToPoint());
        }

        public static Point Transform(this Point value, Matrix matrix)
        {
            return value.ToVector2().Transform(matrix).ToPoint();
        }

        public static XElement ToXml(this Color value, string name)
        {
            return new XElement(name,
                new XAttribute("A", value.A),
                new XAttribute("R", value.R),
                new XAttribute("G", value.G),
                new XAttribute("B", value.B));
        }

        public static Color FromXml(this Color value, XElement xEle)
        {
            return new Color(
                Byte.Parse(xEle.Attribute("A").Value, CultureInfo.InvariantCulture),
                Byte.Parse(xEle.Attribute("R").Value, CultureInfo.InvariantCulture),
                Byte.Parse(xEle.Attribute("G").Value, CultureInfo.InvariantCulture),
                Byte.Parse(xEle.Attribute("B").Value, CultureInfo.InvariantCulture)
            );
        }

        public static void Draw(this SpriteBatch value, Texture2D texture, RectangleF destRect, Rectangle? srcRect = null, Color? color = null, float rotation = 0f, Vector2? origin = null, SpriteEffects spriteEffects = SpriteEffects.None,  float layerDepth = 0f)
        {
            if(!origin.HasValue) origin = Vector2.Zero;
            if (!color.HasValue) color = Color.White;
            Vector2 scale = new Vector2(
                destRect.Width / texture.Width,
                destRect.Height / texture.Height
            );

            value.Draw(texture, destRect.Position, null, color.Value, rotation, origin.Value, scale, SpriteEffects.None, layerDepth);
        }

        public static Vector2 ToVector2(this Coordinate value)
        {
            return new Vector2((float)value.X, -(float)value.Y);
        }

        public static float Cross(this Vector2 v, Vector2 w)
        {
            return v.X * w.Y - v.Y * w.X;
        }
        public static string GetValue(this TagsCollectionBase dict, string key, string defaultValue = null)
        {
            return dict.TryGetValue(key, out string value) ? value : defaultValue;
        }

        public static float Direction(this Vector2 value)
        {
            return (float)Math.Atan2(value.Y, value.X);
        }

        public static IEnumerable<Vector2> FromLineStripToList(this IEnumerable<Vector2> source)
        {
            Vector2[] arr = source.ToArray();

            yield return arr[0];

            for (int i = 1; i < arr.Length - 1; i++)
            {
                yield return arr[i];
                yield return arr[i];
            }

            yield return arr[arr.Length - 1];
        }

        public static Vector2 PerpendicularClockwise(this Vector2 vector2)
        {
            return new Vector2(vector2.Y, -vector2.X);
        }

        public static Vector2 PerpendicularCounterClockwise(this Vector2 vector2)
        {
            return new Vector2(-vector2.Y, vector2.X);
        }

        public static bool Equals(this Vector2 value, Vector2 other, float delta)
        {
            Vector2 diff = other - value;
            return Math.Abs(diff.X) <= delta && Math.Abs(diff.Y) <= delta;
        }

        public static Vector2 Rounded(this Vector2 value)
        {
            value.Round();
            return value;
        }
    }
}

public class EnumUtils
{
    public static IEnumerable<T> GetFlags<T>(T input) where T : Enum
    {
        foreach (T value in Enum.GetValues(input.GetType()))
            if (input.HasFlag(value) && Convert.ToInt32(value) != 0)
                yield return value;
    }
}