﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NauticalRenderer.Data.Map;
using NauticalRenderer.Data.MapPack;
using NauticalRenderer.Graphics;
using NauticalRenderer.Screens;
using NauticalRenderer.SlippyMap.Data;
using NauticalRenderer.Utility;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Streams;
using OsmSharp.Streams.Complete;

namespace NauticalRenderer.SlippyMap.Layers
{
    class ImportantAreaLayer : MapLayer
    {
        private List<RestrictedArea> restrictedAreas;
        private List<Vector2[]> seacables;
        private LineList borders;
        private VertexPositionTexture[] seacablesVerts;

        private Effect squigglyLineEffect;
        private MapScreen mapScreen;

        /// <inheritdoc />
        public ImportantAreaLayer(MapScreen mapScreen)
        {
            this.mapScreen = mapScreen;
        }

        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            squigglyLineEffect = Globals.Content.Load<Effect>("Effects/SquigglyLineEffect");

            OsmCompleteStreamSource source = new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete();

            borders = new LineList(source
                    .OfType<CompleteWay>()
                    .Where(x => x.Tags.Contains("admin_level", "2") &&
                                x.Tags.Contains("boundary", "administrative"))
                    .Select(OsmHelpers.WayToLineStrip)
                    .SelectMany(Utility.Utility.LineStripToLineList)
                    .ToArray()
                , Color.Purple);

            restrictedAreas = source
                    .Where(osmGeo => osmGeo.Tags.Contains("seamark:type", "restricted_area"))
                    .Where(x => x is CompleteWay).Select(x => new RestrictedArea
                        (
                            x.Tags.GetValue("name"),
                            ((CompleteWay)x).Nodes
                                .Select(y => new Vector2((float)y.Longitude, -(float)y.Latitude)).ToArray(),
                            x.Tags.TryGetValue("seamark:restricted_area:category", out string value)
                                ? Enum.TryParse(value, true, out RestrictedArea.RestrictedAreaCategory cat) ? cat :
                                RestrictedArea.RestrictedAreaCategory.UNKNOWN
                                : RestrictedArea.RestrictedAreaCategory.UNKNOWN,
                            x.Tags.TryGetValue("seamark:restricted_area:restriction", out value)
                                ? Enum.TryParse(value, true, out RestrictedArea.RestrictedAreaRestriction r) ? r :
                                RestrictedArea.RestrictedAreaRestriction.UNKNOWN
                                : RestrictedArea.RestrictedAreaRestriction.UNKNOWN,
                            x.Tags
                        ))
                    .ToList();

            seacables = source
                .Where(osmGeo => osmGeo.Tags.Contains("seamark:type", "cable_submarine"))
                .OfType<CompleteWay>()
                .Select(x => x.Nodes
                    .Select(n => new Vector2((float) n.Longitude, -(float) n.Latitude))
                    .ToArray())
                .ToList();

            List<VertexPositionTexture> seacablesVertsList = new List<VertexPositionTexture>();
            foreach (Vector2[] seacable in seacables)
            {
                for (int i = 0; i < seacable.Length - 1; i++)
                {
                    Vector2 start = seacable[i];
                    Vector2 end = seacable[i + 1];

                    Vector2 halfWidthVector = (end - start);
                    float length = (float)Math.Ceiling((halfWidthVector.Length() * 1000) / MathHelper.TwoPi) * MathHelper.TwoPi;
                    
                    halfWidthVector = halfWidthVector.PerpendicularClockwise();
                    halfWidthVector.Normalize();
                    halfWidthVector *= 0.001f;

                    seacablesVertsList.Add(new VertexPositionTexture(new Vector3(start - halfWidthVector, 0), new Vector2(0, -1)));
                    seacablesVertsList.Add(new VertexPositionTexture(new Vector3(start + halfWidthVector, 0), new Vector2(0, 1)));
                    seacablesVertsList.Add(new VertexPositionTexture(new Vector3(end - halfWidthVector, 0), new Vector2(length, -1)));

                    seacablesVertsList.Add(new VertexPositionTexture(new Vector3(start + halfWidthVector, 0), new Vector2(0, 1)));
                    seacablesVertsList.Add(new VertexPositionTexture(new Vector3(end + halfWidthVector, 0), new Vector2(length, 1)));
                    seacablesVertsList.Add(new VertexPositionTexture(new Vector3(end - halfWidthVector, 0), new Vector2(length, -1)));
                }
            }

            seacablesVerts = seacablesVertsList.ToArray();
        }

        private MouseState lastMouseState;
        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            EffectPool.DashedLineEffect.WorldMatrix = camera.GetMatrix();
            EffectPool.DashedLineEffect.LineAndGapLengths = new[] { 10f, 10f, 1f, 10f };
            EffectPool.DashedLineEffect.Apply();
            borders.Draw(mapSb);

            // used so that only the first area that is found is marked when mouse is hovering over it
            bool mouseInPreviousArea = false;
            
            foreach (RestrictedArea restrictedArea in restrictedAreas)
            {
                if (!restrictedArea.BoundingRectangle.Intersects(camera.DrawBounds)) continue;

                if (mouseInPreviousArea)
                {
                    restrictedArea.Draw(sb, mapSb, camera, false);
                }
                else
                {
                    bool mouseInside = restrictedArea.IsArea && restrictedArea.Contains(camera.MousePosition);
                    restrictedArea.Draw(sb, mapSb, camera, mouseInside);
                    if (mouseInside)
                    {
                        mouseInPreviousArea = true;
                        if (Mouse.GetState().LeftButton == ButtonState.Released &&
                            lastMouseState.LeftButton == ButtonState.Pressed)
                        {
                            mapScreen.ShowOsmTagsWindow(restrictedArea.Label, restrictedArea.Tags);
                        }
                    }
                }
            }

            squigglyLineEffect.Parameters["WorldViewProjection"].SetValue(camera.GetMatrix() * Matrix.CreateOrthographicOffCenter(
                                                                              0,
                                                                              Globals.Graphics.GraphicsDevice.Viewport.Width,
                                                                              Globals.Graphics.GraphicsDevice.Viewport.Height,
                                                                              0,
                                                                              0,
                                                                              1));

            foreach (EffectPass pass in squigglyLineEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                mapSb.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, seacablesVerts, 0, seacablesVerts.Length / 3);
            }

            lastMouseState = Mouse.GetState();
        }
    }
}
