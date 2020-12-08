#region Using Statements

using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using NauticalRenderer.Nmea;
using NauticalRenderer.Screens;
using NauticalRenderer.SlippyMap.SourceLayers;
using NauticalRenderer.Utility;

#endregion

namespace NauticalRenderer
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Main : Game
    {
        private readonly Stopwatch frametimeWatch = new Stopwatch();

        public Main(ResourceManager resourceManager, SettingsManager settingsManager)
        {
            Globals.Graphics = new GraphicsDeviceManager(this);
            Globals.Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Globals.ResourceManager = resourceManager;
            Globals.SettingsManager = settingsManager;
            Content.RootDirectory = "Content";
            Globals.Graphics.IsFullScreen = false;
            IsMouseVisible = true;
            Globals.Graphics.PreferredBackBufferWidth = 1280;
            Globals.Graphics.PreferredBackBufferHeight = 720;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            GpsManager.Initialize();

            MyraEnvironment.Game = this;
            ScreenHandler.CurrentScreen = new MapScreen();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Globals.Content = Content;
            Fonts.LoadContent(Content);
            Icons.LoadContent(Content);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // For Mobile devices, this logic will close the Game when the Back button is pressed
            // Exit() is obsolete on iOS
#if !__IOS__ && !__TVOS__
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
#endif

            ScreenHandler.Update(gameTime);
            

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            frametimeWatch.Reset();
            frametimeWatch.Start();

            ScreenHandler.Draw();


            base.Draw(gameTime);

            Window.Title = "Frame Time: " + frametimeWatch.ElapsedMilliseconds + "ms";
        }


    }
}
