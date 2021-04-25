using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myra.Attributes;
using NauticalRenderer.Data;
using NauticalRenderer.Data.MapPack;

namespace NauticalRenderer.SlippyMap.Layers
{
    public abstract class MapLayer
    {
        public abstract ILayerSettings LayerSettings { get; }
        public abstract void LoadContent(MapPack mapPack);
        public abstract void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera);

        public interface ILayerSettings
        {
            
        }
    }
}
