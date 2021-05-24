using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Graphics;
using NauticalRenderer.Utility;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NauticalRenderer.SlippyMap.SourceLayers
{
    class SimplifiedCoastSourceLayer : SourceLayer
    {
        private LineList[] lineLists;

        /// <inheritdoc />
        public override void LoadContent()
        {
            List<(RectangleF boundingRect, Vector2[] points)> polygons = new List<(RectangleF boundingRect, Vector2[] points)>();


            ShapefileDataReader dataReader = Shapefile.CreateDataReader("Content/SimplifiedCoastlines/coastlines-simplified", GeometryFactory.Default);

            while (dataReader.Read())
            {
                Geometry geo = dataReader.Geometry;
                Vector2[] arr = geo.Coordinates.Select(x => new Vector2((float)x.X, -(float)x.Y)).ToArray();
                polygons.Add((OsmHelpers.GetBoundingRectOfPoints(arr), arr));
            }

            polygons.RemoveAll(x => x.points.Length == 0);

            dataReader.Close();

            lineLists = new LineList[polygons.Count];
            for (int i = 0; i < polygons.Count; i++)
            {
                lineLists[i] = new LineList(Utility.Utility.LineStripToLineList(polygons[i].points), Color.LightGray);
            }
        }

        /// <inheritdoc />
        public override void Draw(Camera camera)
        {
            SpriteBatch mapSb = new SpriteBatch(Globals.Graphics.GraphicsDevice);

            EffectPool.BasicEffect.View = camera.GetMatrix();
            EffectPool.BasicEffect.CurrentTechnique.Passes[0].Apply();


            foreach (LineList lineList in lineLists)
            {
                if(lineList.BoundingRectangle.Intersects(camera.DrawBounds))
                    lineList.Draw(mapSb);
            }
        }
    }
}
