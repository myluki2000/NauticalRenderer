using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace NauticalRenderer.Screens
{
    abstract class Screen
    {
        public abstract void Initialize();
        public abstract void Draw();
        public abstract void Update(GameTime gameTime);
    }
}
