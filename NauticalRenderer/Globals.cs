using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using NauticalRenderer.Resources;

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
            set
            {
                _gameWindow = value;
                _gameWindow.ClientSizeChanged += (sender, args) => UpdateViewportMatrix();
            }
        }

        public static event Action ViewportMatrixChanged;

        public static Matrix ViewportMatrix { get; private set; }

        public static void UpdateViewportMatrix()
        {
            ViewportMatrix = Matrix.CreateOrthographicOffCenter(
                0,
                Graphics.GraphicsDevice.Viewport.Width,
                Graphics.GraphicsDevice.Viewport.Height,
                0,
                0,
                1);
            ViewportMatrixChanged?.Invoke();
        }

        private static GameWindow _gameWindow;

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
        
        #endregion
    }
}
