using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Graphics.Effects;

namespace NauticalRenderer.Graphics
{
    public static class EffectPool
    {
        public static BasicEffect BasicEffect { get; } = new BasicEffect(Globals.Graphics.GraphicsDevice);
        public static DashedLineEffect DashedLineEffect { get; private set; }

        public static void LoadContent(ContentManager content)
        {
            DashedLineEffect = new DashedLineEffect(content);

            BasicEffect.VertexColorEnabled = true;

            Globals.ViewportMatrixChanged += () => BasicEffect.Projection = Globals.ViewportMatrix;
            BasicEffect.Projection = Globals.ViewportMatrix;
        }
    }
}
