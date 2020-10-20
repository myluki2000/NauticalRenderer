using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using NauticalRenderer.Utility;

namespace NauticalRenderer.SlippyMap
{
    public class Camera
    {
        private const float DRAW_BOUNDS_MARGIN = 100f;

        /// <summary>
        /// The translation of the camera view.
        /// </summary>
        public Vector3 Translation
        {
            get => translation;
            set
            {
                translation = value;
                viewMatrixValid = false;
                TranslationChanged?.Invoke(this, null);
            }
        }

        /// <summary>
        /// The scale of the camera view.
        /// </summary>
        public Vector3 Scale
        {
            get => scale;
            set
            {
                scale = value;
                viewMatrixValid = false;
            }
        }

        public RectangleF DrawBounds { get; private set; }

        private Vector3 translation;
        private Vector3 scale;

        private Matrix viewMatrix;
        private bool viewMatrixValid = false;

        public event EventHandler TranslationChanged;


        /// <summary>
        /// Create a new camera object with default values. Translation = 0, Scale = 1
        /// </summary>
        public Camera()
        {
            Translation = new Vector3(0, 0, 0);
            Scale = new Vector3(100, 100, 1);
            viewMatrix = GetMatrix();
        }

        /// <summary>
        /// Create a new camera object.
        /// </summary>
        /// <param name="translation">The translation of the camera view</param>
        /// <param name="scale">The scale of the camera view</param>
        public Camera(Vector3 translation, Vector3 scale)
        {
            this.Translation = translation;
            this.Scale = scale;
        }

        /// <returns>Returns a matrix which represents the camera view</returns>
        public Matrix GetMatrix()
        {
            if (!viewMatrixValid)
            {
                viewMatrix = Matrix.CreateTranslation(Translation)
                             * Matrix.CreateScale(Scale)
                             * Matrix.CreateTranslation(1920 / 2, 1080 / 2, 0)
                             * Matrix.CreateScale((float)Globals.Graphics.PreferredBackBufferHeight / 1080);
                Matrix inverted = viewMatrix.Invert();
                Vector2 topLeft = new Vector2(-DRAW_BOUNDS_MARGIN, -DRAW_BOUNDS_MARGIN).Transform(inverted);
                DrawBounds = new RectangleF(
                    topLeft,
                    new Vector2(Globals.Graphics.PreferredBackBufferWidth + DRAW_BOUNDS_MARGIN, Globals.Graphics.PreferredBackBufferHeight + DRAW_BOUNDS_MARGIN).Transform(inverted) - topLeft
                );

                viewMatrixValid = true;
            }

            return viewMatrix;
        }

        public Matrix GetMatrixWithYScale()
        {
            return Matrix.CreateTranslation(Translation)
                   * Matrix.CreateScale(Scale.Y, Scale.Y, 1)
                   * Matrix.CreateTranslation(1920 / 2, 1080 / 2, 0)
                   * Matrix.CreateScale((float)Globals.Graphics.PreferredBackBufferHeight / 1080);
        }

        /// <summary>
        /// Focuses the camera on a position in the world.
        /// </summary>
        /// <param name="focusPos">The position to focus on</param>
        /// <param name="horizontalAlignment">The horizontal alignment of the position the camera should focus on.</param>
        /// <param name="verticalAlignment">The vertical alignment of the position the camera should focus on.</param>
        public void FocusOnPosition(Vector2 focusPos,
                                    HorizontalAlignment horizontalAlignment = HorizontalAlignment.CENTER,
                                    VerticalAlignment verticalAlignment = VerticalAlignment.MIDDLE)
        {
            Vector2 displacement = new Vector2(0, 0);
            switch (horizontalAlignment)
            {
                case HorizontalAlignment.LEFT:
                    displacement.X = -(1920 / 2);
                    break;
                case HorizontalAlignment.CENTER:
                    displacement.X = 0;
                    break;
                case HorizontalAlignment.RIGHT:
                    displacement.X = (1920 / 2);
                    break;
            }

            switch (verticalAlignment)
            {
                case VerticalAlignment.TOP:
                    displacement.Y = -(1080 / 2);
                    break;
                case VerticalAlignment.MIDDLE:
                    displacement.Y = 0;
                    break;
                case VerticalAlignment.BOTTOM:
                    displacement.Y = (1080 / 2);
                    break;
            }

            Translation = new Vector3(-focusPos + displacement, 0);
        }

        public Vector2 ScreenPosToWorldPos(Vector2 screenPos)
        {
            // TODO: This may not work with non-default scale. Check if it does before using
            /*return new Vector2(
                (screenPos.X / Globals.Graphics.PreferredBackBufferWidth) * 1920 / Scale.X - Translation.X,
                (screenPos.Y / Globals.Graphics.PreferredBackBufferHeight) * 1080 / Scale.Y - Translation.Y
            );*/
            return screenPos.Transform(GetMatrix().Invert());
        }

        public static Matrix GetViewportMatrix()
        {
            return Matrix.CreateScale((float)Globals.Graphics.PreferredBackBufferHeight / 1080);
        }

        public enum VerticalAlignment
        {
            TOP,
            MIDDLE,
            BOTTOM
        }

        public enum HorizontalAlignment
        {
            LEFT,
            CENTER,
            RIGHT
        }
    }
}
