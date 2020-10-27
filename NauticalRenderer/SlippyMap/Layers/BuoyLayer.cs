using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Data;
using NauticalRenderer.SlippyMap.Data;
using NauticalRenderer.Utility;
using NetTopologySuite.Utilities;
using OsmSharp;
using OsmSharp.Streams;
using OsmSharp.Streams.Complete;

namespace NauticalRenderer.SlippyMap.Layers
{
    class BuoyLayer : MapLayer
    {
        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        private VertexDeclaration instanceVertexDeclaration;
        private VertexBuffer geometryBuffer;
        private VertexBuffer instanceBuffer;
        private IndexBuffer indexBuffer;
        private InstanceInfo[] instances;

        private VertexBufferBinding[] bindings;

        private Effect buoyEffect;

        public BuoyLayer()
        {

        }

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            buoyEffect = Globals.Content.Load<Effect>("Effects/BuoyEffect");

            instanceVertexDeclaration = new VertexDeclaration(new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 1),
                new VertexElement(sizeof(float) * 2, VertexElementFormat.Byte4, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(sizeof(float) * 2 + 4, VertexElementFormat.Byte4, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(sizeof(float) * 2 + 4 + sizeof(Int32), VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(sizeof(float) * 2 + 4 + sizeof(Int32) + sizeof(byte) * 4, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(sizeof(float) * 2 + 4 + sizeof(Int32) + sizeof(byte) * 4 * 2, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(sizeof(float) * 2 + 4 + sizeof(Int32) + sizeof(byte) * 4 * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            });

            const float HALF_SIZE = 0.5f;
            VertexPositionTexture[] vertices = new VertexPositionTexture[]
            {
                new VertexPositionTexture(new Vector3(-HALF_SIZE, HALF_SIZE, 0), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(-HALF_SIZE, -HALF_SIZE, 0), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(HALF_SIZE, -HALF_SIZE, 0), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(HALF_SIZE, HALF_SIZE, 0), new Vector2(1, 1)),
            };
            geometryBuffer = new VertexBuffer(Globals.Graphics.GraphicsDevice, VertexPositionTexture.VertexDeclaration, 6, BufferUsage.WriteOnly);
            geometryBuffer.SetData(vertices);

            int[] indices = new[]
            {
                0, 1, 3,
                1, 2, 3
            };
            indexBuffer = new IndexBuffer(Globals.Graphics.GraphicsDevice, typeof(int), 6, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);

            List<Node> buoyNodes = new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete()
                .Where(x => x.Tags.ToList().Exists(x => x.Key.StartsWith("seamark:buoy")))
                .OfType<Node>()
                .ToList();

            Buoy[] buoys = buoyNodes
                .Where(x => Enum.TryParse(x.Tags["seamark:type"], true, out Buoy.BuoyType type))
                .Select(x =>
            {
                Buoy.BuoyShape shape = Buoy.BuoyShape.PILLAR;
                bool typeSuccess = Enum.TryParse(x.Tags["seamark:type"], true, out Buoy.BuoyType type);
                bool shapeSuccess = (x.Tags.TryGetValue("seamark:" + type.ToString().ToLower() + ":shape", out string shapeString) || x.Tags.TryGetValue("buoy:shape", out shapeString))
                           && Enum.TryParse(shapeString, true, out shape);

                Debug.Assert(typeSuccess);

                bool colorSuccess = x.Tags.TryGetValue("seamark:" + type.ToString().ToLower() + ":colour", out string colorsString)
                                     || x.Tags.TryGetValue("seamark:" + type.ToString().ToLower() + ":color", out colorsString)
                                     || x.Tags.TryGetValue("buoy:colour", out colorsString)
                                     || x.Tags.TryGetValue("buoy:color", out colorsString);

                Buoy.BuoyColorPattern colorPattern = Buoy.BuoyColorPattern.NONE;
                bool colorPatternSuccess =
                    (x.Tags.TryGetValue("seamark:" + type.ToString().ToLower() + ":colour_pattern", out string patternString)
                    || x.Tags.TryGetValue("seamark:" + type.ToString().ToLower() + ":color_pattern", out patternString)
                    || x.Tags.TryGetValue("seamark:" + type.ToString().ToLower() + ":pattern", out patternString)
                    || x.Tags.TryGetValue("buoy:colour_pattern", out patternString)
                    || x.Tags.TryGetValue("buoy:color_pattern", out patternString))
                    && Enum.TryParse(patternString, true, out colorPattern);

                return new Buoy(new Vector2((float)x.Longitude, -(float)x.Latitude),
                    type,
                    shape,
                    colorPattern,
                    colorSuccess
                        ? colorsString.Split(';').Select(OsmHelpers.GetColorFromSeamarkColor).ToArray()
                        : new Color[] { });
            }).ToArray();

            instances = buoys.Select(x =>
            {
                Debug.Assert(x.Colors.Length <= 4);

                return new InstanceInfo(
                    x.Coordinates,
                    (int)x.Shape,
                    (int)x.ColorPattern,
                    x.Colors.Length >= 1 ? x.Colors[0] : Color.Transparent,
                    x.Colors.Length >= 2 ? x.Colors[1] : Color.Transparent,
                    x.Colors.Length >= 3 ? x.Colors[2] : Color.Transparent,
                    x.Colors.Length >= 4 ? x.Colors[3] : Color.Transparent);
            }).ToArray();
            instanceBuffer = new VertexBuffer(Globals.Graphics.GraphicsDevice, instanceVertexDeclaration, instances.Length, BufferUsage.WriteOnly);
            instanceBuffer.SetData(instances);

            bindings = new[]
            {
                new VertexBufferBinding(geometryBuffer),
                new VertexBufferBinding(instanceBuffer, 0, 1),
            };
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            buoyEffect.CurrentTechnique = buoyEffect.Techniques[0];
            buoyEffect.Parameters["Size"].SetValue(24.0f);
            buoyEffect.Parameters["WorldMatrix"].SetValue(camera.GetMatrix());
            buoyEffect.Parameters["ViewportMatrix"].SetValue(Matrix.CreateOrthographicOffCenter(
                0,
                Globals.Graphics.GraphicsDevice.Viewport.Width,
                Globals.Graphics.GraphicsDevice.Viewport.Height,
                0,
                0,
                1));
            Globals.Graphics.GraphicsDevice.Indices = indexBuffer;
            buoyEffect.Parameters["Texture"].SetValue(Icons.Buoys);
            buoyEffect.CurrentTechnique.Passes[0].Apply();
            Globals.Graphics.GraphicsDevice.SetVertexBuffers(bindings);
            Globals.Graphics.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 2, 0, instances.Length);
        }

        struct InstanceInfo
        {
            public Vector2 InstanceTransform;
            public Int32 BuoyShape;
            public Int32 ColorPattern;
            public Color Color1;
            public Color Color2;
            public Color Color3;
            public Color Color4;

            public InstanceInfo(Vector2 instanceTransform, int buoyShape, int colorPattern, Color color1, Color color2, Color color3, Color color4)
            {
                InstanceTransform = instanceTransform;
                BuoyShape = buoyShape;
                ColorPattern = colorPattern;
                Color1 = color1;
                Color2 = color2;
                Color3 = color3;
                Color4 = color4;
            }
        }
    }
}
