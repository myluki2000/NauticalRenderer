﻿using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace NauticalRenderer.Graphics.Effects
{
    public static class DashedLineEffect
    {
        private static Effect effect;
        private static EffectParameter lineAndGapLengthsParam;
        private static EffectParameter worldMatrixParam;
        private static EffectParameter viewportMatrixParam;
        private static EffectParameter backgroundColorParam;

        public static float[] LineAndGapLengths
        {
            get => lineAndGapLengthsParam.GetValueSingleArray();
            set
            {
                Debug.Assert(value.Length == 4);
                lineAndGapLengthsParam.SetValue(value);
            }
        }

        public static Matrix WorldMatrix
        {
            get => worldMatrixParam.GetValueMatrix();
            set => worldMatrixParam.SetValue(value);
        }

        public static Vector4 BackgroundColor
        {
            get => backgroundColorParam.GetValueVector4();
            set => backgroundColorParam.SetValue(value);
        }

        public static void Initialize(ContentManager content)
        {
            effect = content.Load<Effect>("Effects/DashedLineEffect");
            lineAndGapLengthsParam = effect.Parameters["LineAndGapLengths"];
            worldMatrixParam = effect.Parameters["WorldMatrix"];
            viewportMatrixParam = effect.Parameters["ViewportMatrix"];
            backgroundColorParam = effect.Parameters["BackgroundColor"];

            Globals.ViewportMatrixChanged += () => viewportMatrixParam.SetValue(Globals.ViewportMatrix);
            viewportMatrixParam.SetValue(Globals.ViewportMatrix);
        }

        public static void Apply()
        {
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
