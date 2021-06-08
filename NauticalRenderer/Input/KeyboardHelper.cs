using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace NauticalRenderer.Input
{
    public static class KeyboardHelper
    {
        public static KeyboardState LastKeyBoardState { get; private set; }
        public static KeyboardState CurrentKeyboardState { get; private set; }

        /// <summary>
        /// Manages keyboard updates. MUST BE CALLED AS THE FIRST THING IN THE UPDATE LOOP.
        /// </summary>
        public static void Update(GameTime gameTime)
        {
            LastKeyBoardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();
        }

        public static bool WasPressed(Keys key) =>
            LastKeyBoardState.IsKeyUp(key) && CurrentKeyboardState.IsKeyDown(key);

        public static bool WasReleased(Keys key) =>
            LastKeyBoardState.IsKeyDown(key) && CurrentKeyboardState.IsKeyUp(key);
    }
}
