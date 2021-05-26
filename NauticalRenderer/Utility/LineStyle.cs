using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Graphics;
using NauticalRenderer.Resources;

namespace NauticalRenderer.Utility
{
    public class LineStyle
    {
        public static readonly LineStyle NoFishing = new LineStyle(
            new []
            {
                (SegmentStyle.LINE, 0.001f),
                (SegmentStyle.SPACE, 0.001f),
                (SegmentStyle.LINE, 0.001f),
                (SegmentStyle.SPACE, 0.001f),
                (SegmentStyle.LINE, 0.001f),
                (SegmentStyle.SPACE, 0.001f),
                (SegmentStyle.IMAGE, 0.001f),
                (SegmentStyle.SPACE, 0.001f),
                (SegmentStyle.LINE, 0.001f),
                (SegmentStyle.SPACE, 0.001f),
                (SegmentStyle.LINE, 0.001f),
                (SegmentStyle.SPACE, 0.001f),
            },
            Icons.NoFishing,
            0.001f
        );
        public static readonly LineStyle FishingRestricted = new LineStyle(
            new[]
            {
                (SegmentStyle.LINE, 0.001f),
                (SegmentStyle.SPACE, 0.001f),
                (SegmentStyle.LINE, 0.001f),
                (SegmentStyle.SPACE, 0.001f),
                (SegmentStyle.LINE, 0.001f),
                (SegmentStyle.SPACE, 0.001f),
                (SegmentStyle.IMAGE, 0.001f),
                (SegmentStyle.SPACE, 0.001f),
                (SegmentStyle.LINE, 0.001f),
                (SegmentStyle.SPACE, 0.001f),
                (SegmentStyle.LINE, 0.001f),
                (SegmentStyle.SPACE, 0.001f),
            },
            Icons.RestrictedFishing,
            0.001f
        );
        //public static readonly LineStyle RESTRICTED_AREA_LINE = new LineStyle();

        public float Width { get; }
        public (SegmentStyle style, float length)[] Segments { get; }
        public float Length { get; }
        public Texture2D Image { get; }

        public LineStyle((SegmentStyle style, float length)[] segments, Texture2D image = null, float width = 1.0f)
        {
            Width = width;
            Segments = segments;
            Length = segments.Sum(x => x.length);
            Image = image;
        }

        public enum SegmentStyle
        {
            DOT,
            SPACE,
            LINE,
            IMAGE,
            CROSS,
            CROSS_INSIDE_ONLY,
            CROSS_OUTSIDE_ONLY,
            SQUIGGLY_LINE,
            DOUBLE_LINE,
        }
    }
}
