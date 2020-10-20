using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NauticalRenderer.Utility
{
    public static class Fonts
    {
        public static class Arial
        {
            public static SpriteFont Regular;
        }

        public static void LoadContent(ContentManager content)
        {
            Arial.Regular = content.Load<SpriteFont>("Fonts/Arial-Regular");
        }
    }
}
