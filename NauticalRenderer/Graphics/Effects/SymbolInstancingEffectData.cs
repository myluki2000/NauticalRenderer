using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NauticalRenderer.Graphics.Effects
{
    public class SymbolInstancingEffectData
    {
        private readonly VertexDeclaration instanceVertexDeclaration;
        private readonly VertexBuffer geometryBuffer;
        private readonly IndexBuffer indexBuffer;
        private VertexBuffer instanceBuffer;
        private VertexBufferBinding[] bindings;

        private int instanceCount;

        public SymbolInstancingEffectData()
        {
            instanceVertexDeclaration = new VertexDeclaration(new[]
            {
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 1),
                new VertexElement(sizeof(float) * 2, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
            });

            const float HALF_SIZE = 0.5f;
            VertexPositionTexture[] vertices =
            {
                new(new Vector3(-HALF_SIZE, HALF_SIZE, 0), new Vector2(0, 1)),
                new(new Vector3(-HALF_SIZE, -HALF_SIZE, 0), new Vector2(0, 0)),
                new(new Vector3(HALF_SIZE, -HALF_SIZE, 0), new Vector2(1, 0)),
                new(new Vector3(HALF_SIZE, HALF_SIZE, 0), new Vector2(1, 1)),
            };

            geometryBuffer = new VertexBuffer(Globals.Graphics.GraphicsDevice, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
            geometryBuffer.SetData(vertices);

            int[] indices = {
                0, 1, 3,
                1, 2, 3
            };
            indexBuffer = new IndexBuffer(Globals.Graphics.GraphicsDevice, typeof(int), 6, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
            UpdateBindings();
        }

        public void SetInstanceData(InstanceInfo[] instances)
        {
            instanceBuffer?.Dispose();
            instanceBuffer = new VertexBuffer(Globals.Graphics.GraphicsDevice, instanceVertexDeclaration, instances.Length, BufferUsage.WriteOnly);
            instanceBuffer.SetData(instances);
            instanceCount = instances.Length;
            UpdateBindings();
        }

        public void Draw()
        {
            Globals.Graphics.GraphicsDevice.Indices = indexBuffer;
            SymbolInstancingEffect.Apply();
            if (bindings == null) throw new Exception("Instance and geometry buffers have to be set before Draw() can be called.");
            Globals.Graphics.GraphicsDevice.SetVertexBuffers(bindings);
            Globals.Graphics.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 2, 0, instanceCount);
        }

        private void UpdateBindings()
        {
            if (instanceBuffer != null && geometryBuffer != null)
            {
                bindings = new[] { new VertexBufferBinding(geometryBuffer), new VertexBufferBinding(instanceBuffer, 0, 1) };
            }
            else
            {
                bindings = null;
            }
        }

        public struct InstanceInfo
        {
            public readonly Vector2 InstanceTransform;
            public readonly Vector2 TextureAtlasCoords;

            public InstanceInfo(Vector2 instanceTransform, Vector2 textureAtlasCoords)
            {
                InstanceTransform = instanceTransform;
                TextureAtlasCoords = textureAtlasCoords;
            }
        }
    }
}
