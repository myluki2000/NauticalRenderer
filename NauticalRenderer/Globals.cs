using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace NauticalRenderer
{
    static class Globals
    {
        public static GraphicsDeviceManager Graphics;
        public static ContentManager Content;
        public static ResourceManager ResourceManager;
        public static SettingsManager SettingsManager;

        public static GameWindow GameWindow
        {
            get => _gameWindow;
            set
            {
                _gameWindow = value;
                _gameWindow.ClientSizeChanged += (sender, args) =>
                {
                    ViewportMatrix = Matrix.CreateOrthographicOffCenter(
                        0,
                        Graphics.GraphicsDevice.Viewport.Width,
                        Graphics.GraphicsDevice.Viewport.Height,
                        0,
                        0,
                        1);
                };
            }
        }


        public static Matrix ViewportMatrix { get; private set; }

        #region Texture Coordinates

        public static readonly Rectangle TEXINDEX_0_0 = new Rectangle(0, 0, 512, 512);
        public static readonly Rectangle TEXINDEX_1_0 = new Rectangle(512, 0, 512, 512);
        public static readonly Rectangle TEXINDEX_2_0 = new Rectangle(1024, 0, 512, 512);
        public static readonly Rectangle TEXINDEX_3_0 = new Rectangle(1536, 0, 512, 512);
        public static readonly Rectangle TEXINDEX_0_1 = new Rectangle(0, 512, 512, 512);
        public static readonly Rectangle TEXINDEX_1_1 = new Rectangle(512, 512, 512, 512);
        public static readonly Rectangle TEXINDEX_2_1 = new Rectangle(1024, 512, 512, 512);
        public static readonly Rectangle TEXINDEX_3_1 = new Rectangle(1536, 512, 512, 512);
        public static readonly Rectangle TEXINDEX_0_2 = new Rectangle(0, 1024, 512, 512);
        public static readonly Rectangle TEXINDEX_1_2 = new Rectangle(512, 1024, 512, 512);
        public static readonly Rectangle TEXINDEX_2_2 = new Rectangle(1024, 1024, 512, 512);
        public static readonly Rectangle TEXINDEX_3_2 = new Rectangle(1536, 1024, 512, 512);
        public static readonly Rectangle TEXINDEX_0_3 = new Rectangle(0, 1536, 512, 512);
        public static readonly Rectangle TEXINDEX_1_3 = new Rectangle(512, 1536, 512, 512);
        public static readonly Rectangle TEXINDEX_2_3 = new Rectangle(1024, 1536, 512, 512);
        public static readonly Rectangle TEXINDEX_3_3 = new Rectangle(1536, 1536, 512, 512);
        private static GameWindow _gameWindow;

        #endregion
    }
}
