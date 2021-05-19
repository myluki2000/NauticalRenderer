using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace NauticalRenderer.Effects
{
    public class DashedLineEffect
    {
        private readonly Effect effect;
        private readonly EffectParameter lineAndGapLengthsParam;
        private readonly EffectParameter worldMatrixParam;

        public float[] LineAndGapLengths
        {
            get => lineAndGapLengthsParam.GetValueSingleArray();
            set
            {
                Debug.Assert(value.Length == 4);
                lineAndGapLengthsParam.SetValue(value);
            }
        }

        public Matrix WorldMatrix
        {
            get => worldMatrixParam.GetValueMatrix();
            set => worldMatrixParam.SetValue(value);
        }

        public DashedLineEffect(ContentManager content)
        {
            effect = content.Load<Effect>("Effects/DashedLineEffect");
            lineAndGapLengthsParam = effect.Parameters["lineAndGapLengths"];
            worldMatrixParam = effect.Parameters["WorldMatrix"];

            Globals.ViewportMatrixChanged += () => effect.Parameters["ViewportMatrix"].SetValue(Globals.ViewportMatrix);
            effect.Parameters["ViewportMatrix"].SetValue(Globals.ViewportMatrix);
        }

        public void Apply()
        {
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
