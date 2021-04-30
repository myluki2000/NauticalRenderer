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
using NetTopologySuite.Utilities;

namespace NauticalRenderer.Data
{
    public struct Mesh
    {
        private readonly VertexBuffer vbf;
        private readonly IndexBuffer ibf;
        private readonly int primitiveCount;
        private readonly int vertexCount;
        private readonly int outlineVertexCount;

        public bool DrawOutline { get; set; }

        public RectangleF BoundingRectangle { get; private set; }

        public Mesh(Vector2[] vertices, Color color) : this(vertices, null, color) { }

        public Mesh(Vector2[] vertices, int[] triangles, Color color, bool drawOutline = false, Color? outlineColor = null, Vector2[] outlineVertices = null) : this()
        {
            vertexCount = vertices.Length;
            outlineVertexCount = outlineVertices?.Length ?? 0;
          

            List<VertexPositionColor> vertexData = new List<VertexPositionColor>(vertexCount + outlineVertexCount);
            vertexData.AddRange(vertices.Select(x => new VertexPositionColor(new Vector3(x, 0), color)));

            if(outlineVertices != null) vertexData.AddRange(outlineVertices.Select(x => new VertexPositionColor(new Vector3(x, 0), outlineColor ?? Color.Black)));

            vbf = new VertexBuffer(Globals.Graphics.GraphicsDevice, typeof(VertexPositionColor), vertexCount + outlineVertexCount, BufferUsage.WriteOnly);
            vbf.SetData(vertexData.ToArray());

            if (triangles != null)
            {
                ibf = new IndexBuffer(Globals.Graphics.GraphicsDevice, IndexElementSize.ThirtyTwoBits, triangles.Length, BufferUsage.WriteOnly);
                ibf.SetData(triangles);
                primitiveCount = triangles.Length / 3;
            }
            else
            {
                primitiveCount = vertices.Length / 3;
            }

            DrawOutline = false;

            BoundingRectangle = OsmHelpers.GetBoundingRectOfPoints(vertices);

            DrawOutline = drawOutline;
        }

        public void Draw(SpriteBatch sb, Matrix viewMatrix)
        {
            sb.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Utility.Utility.basicEffect.View = viewMatrix;
            Utility.Utility.basicEffect.CurrentTechnique.Passes[0].Apply();

            sb.GraphicsDevice.SetVertexBuffer(vbf);

            if (ibf == null)
            {
                sb.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, primitiveCount);
            }
            else
            {
                sb.GraphicsDevice.Indices = ibf;
                sb.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
            }

            if (DrawOutline)
            {
                sb.GraphicsDevice.DrawPrimitives(PrimitiveType.LineStrip, vertexCount, outlineVertexCount - 1);
            }
        }
    }
}
