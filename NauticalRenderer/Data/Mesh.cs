using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NauticalRenderer.SlippyMap;
using NauticalRenderer.Utility;
using NetTopologySuite.Index.Quadtree;

namespace NauticalRenderer.Data
{
    public struct Mesh
    {
        private VertexPositionColor[] vertices;
        private Color color;
        public int[] Triangles { get; set; }

        public VertexPositionColor[] Vertices => vertices;

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                if (vertices != null)
                {
                    for(int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i].Color = color;
                    }
                }
            }
        }

        public bool DrawOutline { get; set; }
        public Color OutlineColor { get; set; }
        public Vector2[] OutlineVertices { get; set; }

        public RectangleF BoundingRectangle { get; private set; }

        public Mesh(Vector2[] vertices) : this(vertices, null) { }

        public Mesh(Vector2[] vertices, Color color) : this(vertices, null, color) { }

        public Mesh(Vector2[] vertices, int[] triangles) : this(vertices, triangles, Color.Black) { }

        public Mesh(Vector2[] vertices, int[] triangles, Color color, bool drawOutline = false, Vector2[] outlineVertices = null) : this()
        {
            DrawOutline = false;
            Color = color;
            this.vertices = vertices.Select(x => new VertexPositionColor(new Vector3(x, 0), color)).ToArray();

            BoundingRectangle = OsmHelpers.GetBoundingRectOfPoints(vertices);

            Triangles = triangles;
            DrawOutline = drawOutline;
            OutlineVertices = outlineVertices;
            OutlineColor = Color.Black;
        }

        public void Draw(SpriteBatch sb, Matrix viewMatrix)
        {
            sb.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Utility.Utility.basicEffect.View = viewMatrix;
            Utility.Utility.basicEffect.CurrentTechnique.Passes[0].Apply();

            if (Triangles == null)
                sb.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList,
                    vertices,
                    0,
                    vertices.Length / 3);
            else
                sb.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                    vertices,
                    0,
                    vertices.Length,
                    Triangles,
                    0,
                    Triangles.Length / 3);


            if (DrawOutline)
            {
                LineRenderer.DrawLineStrip(sb,
                    OutlineVertices ?? Vertices.Select(x => x.Position.XY()).ToArray(),
                    OutlineColor,
                    viewMatrix);
            }
        }
    }
}
