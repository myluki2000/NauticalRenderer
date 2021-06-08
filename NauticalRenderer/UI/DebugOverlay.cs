using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NauticalRenderer.Resources;

namespace NauticalRenderer.UI
{
    public static class DebugOverlay
    {
        private static SpriteBatch sb;
        public static bool IsVisible { get; set; }

        private static Stopwatch stopwatch = new Stopwatch();
        private const int LAST_FRAME_TIMES_COUNT = 30;
        private static double[] lastFrameTimes = new double[LAST_FRAME_TIMES_COUNT];
        private static int lastFrameTimesIndex = 0;

        public static void Initialize()
        {
            sb = new SpriteBatch(Globals.Graphics.GraphicsDevice);
        }

        public static void Draw()
        {
            if (!IsVisible) return;

            GraphicsMetrics metrics = Globals.Graphics.GraphicsDevice.Metrics;

            StringBuilder s = new StringBuilder();
            s.Append("Frame Time: ");
            s.Append(lastFrameTimes[lastFrameTimesIndex].ToString("#00.000"));
            s.Append("ms (Avg. ");
            s.Append(lastFrameTimes.Average().ToString("#00.000"));
            s.Append("ms)\n");

            s.Append("Draw Calls: ");
            s.Append(metrics.DrawCount);
            s.Append("\n");

            s.Append("Primitive Count: ");
            s.Append(metrics.PrimitiveCount);
            s.Append("\n");

            s.Append("Sprite Count: ");
            s.Append(metrics.SpriteCount);


            Vector2 size = Fonts.Arial.Regular.MeasureString(s.ToString());

            sb.Begin();
            
            // draw background
            Utility.Utility.DrawRectangle(sb, new Rectangle(0, 0, (int)size.X + 10, (int)size.Y + 10), Color.Black * 0.7f);

            // draw text
            sb.DrawString(Fonts.Arial.Regular, s.ToString(), new Vector2(5, 5), Color.White);

            sb.End();
        }

        public static void StartFrameTimeMeasure()
        {
            stopwatch.Restart();
        }

        public static void StopFrameTimeMeasure()
        {
            stopwatch.Stop();
            lastFrameTimesIndex = (lastFrameTimesIndex + 1) % LAST_FRAME_TIMES_COUNT;
            lastFrameTimes[lastFrameTimesIndex] = stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
