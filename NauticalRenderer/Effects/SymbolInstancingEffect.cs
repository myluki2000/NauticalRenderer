using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NauticalRenderer.Effects
{
    class SymbolInstancingEffect
    {
        private readonly Effect symbolInstancingEffect;

        public Matrix WorldMatrix
        {
            get => symbolInstancingEffect.Parameters["WorldMatrix"].GetValueMatrix();
            set => symbolInstancingEffect.Parameters["WorldMatrix"].SetValue(value);
        }

        public Texture2D Texture
        {
            get => symbolInstancingEffect.Parameters["Texture"].GetValueTexture2D();
            set => symbolInstancingEffect.Parameters["Texture"].SetValue(value);
        }

        public float Size
        {
            get => symbolInstancingEffect.Parameters["Size"].GetValueSingle();
            set => symbolInstancingEffect.Parameters["Size"].SetValue(value);
        }

        public SymbolInstancingEffect(ContentManager content)
        {
            symbolInstancingEffect = Globals.Content.Load<Effect>("Effects/SymbolInstancingEffect");
            Globals.GameWindow.ClientSizeChanged += (sender, args) =>
                symbolInstancingEffect.Parameters["ViewportMatrix"].SetValue(Globals.ViewportMatrix);

        }

        public void Apply()
        {
            for(int i = 0; i < symbolInstancingEffect.CurrentTechnique.Passes.Count; ++i)
                symbolInstancingEffect.CurrentTechnique.Passes[i].Apply();
        }
    }
}
