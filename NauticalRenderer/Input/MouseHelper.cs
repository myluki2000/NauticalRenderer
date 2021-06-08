using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NauticalRenderer.Utility;

namespace NauticalRenderer.Input
{
    public static class MouseHelper
    {
        public static bool LeftButtonClicked { get; private set; }
        public static bool HasUnhandledLeftClick => LeftButtonClicked && !wasLeftClickHandled;
        private static bool wasLeftClickHandled = false;

        public static bool RightButtonClicked { get; private set; }
        public static bool HasUnhandledRightClick => RightButtonClicked && !wasRightClickHandled;
        private static bool wasRightClickHandled = false;

        public static bool HasUnhandledHover { get; private set; }

        private static MouseState lastMouseState;
        private static Point mousePosAtLeftButtonDown;
        private static Point mousePosAtRightButtonDown;

        private const float CLICK_TOLERANCE = 5;

        /// <summary>
        /// Manages mouse updates. MUST BE CALLED AS THE FIRST THING IN THE UPDATE LOOP.
        /// </summary>
        public static void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            HasUnhandledHover = true;
            LeftButtonClicked = false;
            RightButtonClicked = false;
            wasLeftClickHandled = false;
            wasRightClickHandled = false;

            if (lastMouseState.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed)
                mousePosAtLeftButtonDown = mouseState.Position;

            if (lastMouseState.LeftButton == ButtonState.Pressed
                && mouseState.LeftButton == ButtonState.Released
                && (mouseState.Position - mousePosAtLeftButtonDown).LengthSquared() < CLICK_TOLERANCE)
                LeftButtonClicked = true;


            if (lastMouseState.RightButton == ButtonState.Released && mouseState.RightButton == ButtonState.Pressed)
                mousePosAtRightButtonDown = mouseState.Position;

            if (lastMouseState.RightButton == ButtonState.Pressed
                && mouseState.RightButton == ButtonState.Released
                && (mouseState.Position - mousePosAtRightButtonDown).LengthSquared() < CLICK_TOLERANCE)
                RightButtonClicked = true;

            lastMouseState = mouseState;
        }

        public static void LeftClickWasHandled() => wasLeftClickHandled = true;
        public static void RightClickWasHandled() => wasRightClickHandled = true;
        public static void HoverWasHandled() => HasUnhandledHover = false;
    }
}
