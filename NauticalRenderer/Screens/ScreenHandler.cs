using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace NauticalRenderer.Screens
{
    static class ScreenHandler
    {
        private static Screen _currentScreen;

        public static Screen CurrentScreen
        {
            get => _currentScreen;
            set
            {
                _currentScreen = value;
                _currentScreen.Initialize();
            }
        }

        public static void Draw()
        {
            CurrentScreen.Draw();
        }

        public static void Update(GameTime gameTime)
        {
            CurrentScreen.Update(gameTime);
        }
    }
}
