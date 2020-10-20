using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NauticalRenderer.Utility;

namespace NauticalRenderer.SlippyMap.Data
{
    struct Way
    {
        public Vector2[] Points { get; }
        public RectangleF BoundingRectangle { get; }

        /// <inheritdoc />
        public Way(Vector2[] points) : this()
        {
            Points = points;
            
            Vector2 min = new Vector2(
                Points.Min(x => x.X),
                Points.Min(x => x.Y)
            );
            Vector2 max = new Vector2(
                Points.Max(x => x.X),
                Points.Max(x => x.Y)
            );
            BoundingRectangle = new RectangleF(min, max - min);
        }
    }
}
