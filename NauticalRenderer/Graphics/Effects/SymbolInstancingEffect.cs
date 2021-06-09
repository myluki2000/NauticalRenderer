using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.XAudio2;

namespace NauticalRenderer.Graphics.Effects
{
    public static class SymbolInstancingEffect
    {
        private static Effect effect;
        private static EffectParameter worldMatrixParam;
        private static EffectParameter textureParam;
        private static EffectParameter sizeParam;
        private static EffectParameter atlasWidthParam;
        private static EffectParameter viewportMatrixParam;
        
        public static Matrix WorldMatrix
        {
            get => worldMatrixParam.GetValueMatrix();
            set => worldMatrixParam.SetValue(value);
        }

        public static Texture2D Texture
        {
            get => textureParam.GetValueTexture2D();
            set => textureParam.SetValue(value);
        }

        public static float Size
        {
            get => sizeParam.GetValueSingle();
            set => sizeParam.SetValue(value);
        }

        public static int AtlasWidth
        {
            get => atlasWidthParam.GetValueInt32();
            set => atlasWidthParam.SetValue(value);
        }

        public static void Initialize(ContentManager content)
        {
            effect = content.Load<Effect>("Effects/SymbolInstancingEffect");

            worldMatrixParam = effect.Parameters["WorldMatrix"];
            textureParam = effect.Parameters["Texture"];
            sizeParam = effect.Parameters["Size"];
            atlasWidthParam = effect.Parameters["AtlasWidth"];
            viewportMatrixParam = effect.Parameters["ViewportMatrix"];

            Globals.ViewportMatrixChanged += () =>
                viewportMatrixParam.SetValue(Globals.ViewportMatrix);
            viewportMatrixParam.SetValue(Globals.ViewportMatrix);
        }

        public static void Apply()
        {
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
