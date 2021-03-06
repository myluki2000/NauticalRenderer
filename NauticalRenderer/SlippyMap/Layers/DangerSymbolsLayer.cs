﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Data;
using NauticalRenderer.Data.MapPack;
using NauticalRenderer.Graphics;
using NauticalRenderer.Graphics.Effects;
using NauticalRenderer.Resources;
using NauticalRenderer.Utility;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Streams;

namespace NauticalRenderer.SlippyMap.Layers
{
    class DangerSymbolsLayer : MapLayer
    {
        private SymbolInstancingEffectData effectData;

        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }



        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            IEnumerable<ICompleteOsmGeo> dangerSpots = new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete()
                .Where(x => x.Tags.Contains("seamark:type", "rock") || x.Tags.ContainsKey("seamark:rock:water_level")
                            || x.Tags.Contains("seamark:type", "wreck") || x.Tags.ContainsKey("seamark:wreck:category"));

            effectData = new();
            effectData.SetInstanceData(dangerSpots.Select(x =>
            {
                Vector2 atlasCoords = new Vector2(0, 0);
                if (x.Tags.TryGetValue("seamark:rock:water_level", out string value))
                {
                    atlasCoords.X = value switch
                    {
                        "covers" => Icons.DangerSymbols.RockCovers.X,
                        "awash" => Icons.DangerSymbols.RockAwash.X,
                        _ => atlasCoords.X
                    };
                }
                else if (x.Tags.Contains("seamark:type", "wreck"))
                {
                    atlasCoords.Y = (float)Icons.DangerSymbols.Wreck.Y / Icons.DangerSymbols.Texture.Height;
                    if (x.Tags.TryGetValue("seamark:wreck:category", out value))
                    {
                        switch (value)
                        {
                            case "non-dangerous":
                            case "distributed_remains":
                                atlasCoords.X = Icons.DangerSymbols.Wreck.X;
                                break;
                            case "dangerous":
                            case "mast_showing":
                                atlasCoords.X = Icons.DangerSymbols.WreckDangerous.X;
                                break;
                            case "hull_showing":
                                atlasCoords.X = Icons.DangerSymbols.WreckShowing.X;
                                break;
                        }
                    }
                }

                atlasCoords.X /= Icons.DangerSymbols.Texture.Width;
                SymbolInstancingEffectData.InstanceInfo instance =
                    new SymbolInstancingEffectData.InstanceInfo(OsmHelpers.GetCoordinateOfOsmGeo(x), atlasCoords);
                return instance;
            }).ToArray());
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            if (camera.Scale.Y > 3000)
            {
                SymbolInstancingEffect.Size = 24f;
                SymbolInstancingEffect.Texture = Icons.DangerSymbols.Texture;
                SymbolInstancingEffect.WorldMatrix = camera.GetMatrix();
                SymbolInstancingEffect.AtlasWidth = 4;
                effectData.Draw();
            }
        }
    }
}
