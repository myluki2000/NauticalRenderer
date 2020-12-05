using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Utility;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NauticalRenderer.SlippyMap.SourceLayers
{
    class SimplifiedCoastSourceLayer : SourceLayer
    {
        private readonly List<(RectangleF boundingRect, Vector2[] points)> polygons = new List<(RectangleF boundingRect, Vector2[] points)>();

        /// <inheritdoc />
        public override void LoadContent()
        {
            polygons.Clear();

#if !ANDROID
            ShapefileDataReader dataReader = Shapefile.CreateDataReader("Content/SimplifiedCoastlines/coastlines-simplified", GeometryFactory.Default);

            while (dataReader.Read())
            {
                Geometry geo = dataReader.Geometry;
                Vector2[] arr = geo.Coordinates.Select(x => new Vector2((float) x.X, -(float) x.Y)).ToArray();
                polygons.Add((OsmHelpers.GetBoundingRectOfPoints(arr), arr));
            }

            polygons.RemoveAll(x => x.points.Length == 0);

            dataReader.Close();
#endif
        }

        /// <inheritdoc />
        public override void Draw(Camera camera)
        {
            SpriteBatch mapSb = new SpriteBatch(Globals.Graphics.GraphicsDevice);
            mapSb.Begin(transformMatrix: camera.GetMatrix());
            int i = 0;
            foreach ((RectangleF boundingRect, Vector2[] points) in polygons)
            {
                if (boundingRect.Intersects(camera.DrawBounds))
                {
                    LineRenderer.DrawLineStrip(mapSb, points, Color.LightGray, camera.GetMatrix());
                    i++;
                }
            }
            Console.WriteLine("Drawn polys: " + i);
        }
    }
}
