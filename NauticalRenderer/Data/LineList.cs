using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Utility;

namespace NauticalRenderer.Data
{
    struct LineList
    {
        public RectangleF BoundingRectangle { get; }

        private readonly VertexBuffer vbf;

        public LineList(Vector2[] vertices, Color color) : this(vertices.Select(x => new VertexPositionColor(new Vector3(x, 0), color)).ToArray()) { }

        public LineList(VertexPositionColor[] vertices)
        {
            vbf = new VertexBuffer(Globals.Graphics.GraphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
            vbf.SetData(vertices);

            BoundingRectangle = OsmHelpers.GetBoundingRectOfPoints(vertices);
        }

        public void Draw(SpriteBatch sb, Matrix viewMatrix)
        {
            sb.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Utility.Utility.basicEffect.View = viewMatrix;
            Utility.Utility.basicEffect.CurrentTechnique.Passes[0].Apply();

            sb.GraphicsDevice.SetVertexBuffer(vbf);

            sb.GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, vbf.VertexCount - 1);
        }
    }
}
