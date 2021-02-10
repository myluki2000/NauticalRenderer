using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Data;
using NauticalRenderer.Effects;

namespace NauticalRenderer.SlippyMap.Layers
{
    class DangerSymbolsLayer : MapLayer
    {
        private SymbolInstancingEffect symbolEffect;

        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }



        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            symbolEffect = new SymbolInstancingEffect(Globals.Content);
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            symbolEffect.WorldMatrix = camera.GetMatrix();
            symbolEffect.Apply();

        }
    }
}
