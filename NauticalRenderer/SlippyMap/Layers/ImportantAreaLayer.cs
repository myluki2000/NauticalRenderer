using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Myra.Attributes;
using NauticalRenderer.Data;
using NauticalRenderer.SlippyMap.Data;
using NauticalRenderer.Utility;
using OsmSharp.Complete;
using OsmSharp.Streams;
using OsmSharp.Streams.Complete;

namespace NauticalRenderer.SlippyMap.Layers
{
    class ImportantAreaLayer : MapLayer
    {
        private List<RestrictedArea> restrictedAreas;
        private List<Vector2[]> seacables;
        private List<Vector2[]> borders;

        private VertexPositionTexture[] seacablesVerts;

        private Effect squigglyLineEffect;

        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            squigglyLineEffect = Globals.Content.Load<Effect>("Effects/SquigglyLineEffect");

            OsmCompleteStreamSource source = new PBFOsmStreamSource(mapPack.OpenFile("base.osm.pbf")).ToComplete();

            borders = OsmHelpers.WaysToListOfVector2Arr(source
                .Where(osmGeo =>
                    osmGeo.Tags.Contains("admin_level", "2") && osmGeo.Tags.Contains("boundary", "administrative"))
                .ToArray());
            
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
                                : RestrictedArea.RestrictedAreaRestriction.UNKNOWN
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
                    float length = halfWidthVector.Length();

                    halfWidthVector = halfWidthVector.PerpendicularClockwise();
                    halfWidthVector.Normalize();
                    halfWidthVector *= 0.001f;

                    seacablesVertsList.Add(new VertexPositionTexture(new Vector3(start - halfWidthVector, 0), new Vector2(0, 0)));
                    seacablesVertsList.Add(new VertexPositionTexture(new Vector3(start + halfWidthVector, 0), new Vector2(0, 1)));
                    seacablesVertsList.Add(new VertexPositionTexture(new Vector3(end - halfWidthVector, 0), new Vector2(length, 0)));

                    seacablesVertsList.Add(new VertexPositionTexture(new Vector3(start + halfWidthVector, 0), new Vector2(0, 1)));
                    seacablesVertsList.Add(new VertexPositionTexture(new Vector3(end + halfWidthVector, 0), new Vector2(length, 1)));
                    seacablesVertsList.Add(new VertexPositionTexture(new Vector3(end - halfWidthVector, 0), new Vector2(length, 0)));
                }
            }

            seacablesVerts = seacablesVertsList.ToArray();
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            foreach (Vector2[] line in borders)
            {
                LineRenderer.DrawDashedLine(mapSb, line, Color.Purple, new []{0.001f, 0.001f, 0.0001f, 0.001f}, camera.GetMatrix());
            }

            foreach (RestrictedArea restrictedArea in restrictedAreas)
            {
                if(restrictedArea.BoundingRectangle.Intersects(camera.DrawBounds))
                    restrictedArea.Draw(sb, mapSb, camera);
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
        }
    }
}
