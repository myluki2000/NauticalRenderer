using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Data;

namespace NauticalRenderer.SlippyMap.Layers
{
    class StreetLayer : MapLayer
    {
        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            throw new NotImplementedException();
        }
    }
}
