using System;
using System.Collections.Generic;
using System.Text;

namespace NauticalRenderer.SlippyMap.SourceLayers
{
    abstract class SourceLayer
    {
        public abstract void LoadContent();
        public abstract void Draw(Camera camera);
    }
}
