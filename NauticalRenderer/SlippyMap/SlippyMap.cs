using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using NauticalRenderer.Data;
using NauticalRenderer.SlippyMap.Data;
using NauticalRenderer.SlippyMap.Layers;
using NauticalRenderer.SlippyMap.SourceLayers;
using OsmSharp.Streams;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;

namespace NauticalRenderer.SlippyMap
{
    class SlippyMap
    {
        public Camera Camera { get; set; } = new Camera();

        public MapPack MapPack { get; set; }

        public MapSettings Settings { get; set; } = new MapSettings();

        public List<SourceLayer> SourceLayers = new List<SourceLayer>() { new SimplifiedCoastSourceLayer() };

        #region Layers
        private readonly HarbourLayer harbourLayer = new HarbourLayer();
        private readonly LandmarkLayer landmarkLayer = new LandmarkLayer();
        private readonly NavigationLineLayer navigationLineLayer = new NavigationLineLayer();
        private readonly ImportantAreaLayer importantAreaLayer = new ImportantAreaLayer();
        private readonly GribLayer gribLayer = new GribLayer();
        private readonly SeparationSchemeLayer separationSchemeLayer = new SeparationSchemeLayer();
        private readonly MapGeoLayer mapGeoLayer = new MapGeoLayer();
        private readonly PlacenameLayer placenameLayer = new PlacenameLayer();
        private readonly BuoyLayer buoyLayer = new BuoyLayer();
        #endregion


        public SlippyMap()
        {
            Camera.TranslationChanged += (sender, e) => { CorrectScaling(); };
            MapPack = new MapPack("Content/German-Baltic-Coast-And-South-Denmark.mappack");
        }

        
        public void Update(GameTime gameTime)
        {
            HandleMouseInput(gameTime);
            HandleTouchInput(gameTime);
        }

        private TouchCollection lastTouchState;
        private bool touchIsDragging = false;
        private Vector2 touchInertia;
        private void HandleTouchInput(GameTime gameTime)
        {
            TouchCollection touchState = TouchPanel.GetState();

            /*int scrollDelta = mouseState.ScrollWheelValue - lastMouseState.ScrollWheelValue;
            if (scrollDelta > 0)
            {
                Camera.Scale *= new Vector3(1.1f, 1.1f, 1);
            }
            else if (scrollDelta < 0)
            {
                Camera.Scale /= new Vector3(1.1f, 1.1f, 1);
            }*/

            if (touchState.Count == 1 && lastTouchState.Count == 0)
            {
                touchIsDragging = true;
            }
            else if (touchIsDragging && touchState.Count == 0)
            {
                touchIsDragging = false;
            }

            if (Math.Abs(touchInertia.X) + Math.Abs(touchInertia.Y) < 0.001f)
            {
                touchInertia = Vector2.Zero;
            }

            if (touchIsDragging)
            {
                if (lastTouchState.Count == 1)
                {
                    Vector2 delta = touchState[0].Position - lastTouchState[0].Position;
                    delta *= new Vector2(1920.0f / Globals.Graphics.PreferredBackBufferWidth, 1080.0f / Globals.Graphics.PreferredBackBufferHeight);
                    Camera.Translation += new Vector3(delta, 0) / Camera.Scale;

                    touchInertia = (Camera.ScreenPosToWorldPos(touchState[0].Position) - Camera.ScreenPosToWorldPos(lastTouchState[0].Position))
                                   / (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
            }
            else if (touchInertia != Vector2.Zero)
            {
                Camera.Translation += new Vector3(touchInertia * (float)gameTime.ElapsedGameTime.TotalSeconds, 0);
                touchInertia *= 0.85f;
            }

            lastTouchState = touchState;
        }

        private MouseState lastMouseState;
        private bool mouseIsDragging = false;
        private Vector2 mouseInertia;
        private void HandleMouseInput(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            int scrollDelta = mouseState.ScrollWheelValue - lastMouseState.ScrollWheelValue;
            if (scrollDelta > 0)
            {
                Camera.Scale *= new Vector3(1.1f, 1.1f, 1);
            }
            else if (scrollDelta < 0)
            {
                Camera.Scale /= new Vector3(1.1f, 1.1f, 1);
            }


            if (mouseState.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released)
            {
                mouseIsDragging = true;
            }
            else if (mouseIsDragging && mouseState.LeftButton == ButtonState.Released)
            {
                mouseIsDragging = false;
                mouseInertia = (Camera.ScreenPosToWorldPos(mouseState.Position.ToVector2()) - Camera.ScreenPosToWorldPos(lastMouseState.Position.ToVector2()))
                          / (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (Math.Abs(mouseInertia.X) + Math.Abs(mouseInertia.Y) < 0.001f)
            {
                mouseInertia = Vector2.Zero;
            }

            if (mouseIsDragging)
            {
                Vector2 delta = (mouseState.Position - lastMouseState.Position).ToVector2();
                delta *= new Vector2(1920.0f / Globals.Graphics.PreferredBackBufferWidth, 1080.0f / Globals.Graphics.PreferredBackBufferHeight);
                Camera.Translation += new Vector3(delta, 0) / Camera.Scale;
            }
            else if (mouseInertia != Vector2.Zero)
            {
                Camera.Translation += new Vector3(mouseInertia * (float)gameTime.ElapsedGameTime.TotalSeconds, 0);
                mouseInertia *= 0.85f;
            }

            lastMouseState = mouseState;
        }

        private bool first = true;
        public void Draw(SpriteBatch sb)
        {

            foreach (SourceLayer sourceLayer in SourceLayers)
            {
                sourceLayer.Draw(Camera);
            }

            SpriteBatch mapSb = new SpriteBatch(Globals.Graphics.GraphicsDevice);

            mapSb.Begin(transformMatrix: Camera.GetMatrix());

            mapGeoLayer.Draw(sb, mapSb, Camera);
            harbourLayer.Draw(sb, mapSb, Camera);
            landmarkLayer.Draw(sb, mapSb, Camera);
            if(Settings.VisibleLayers.NavigationLineLayer) navigationLineLayer.Draw(sb, mapSb, Camera);
            if(Settings.VisibleLayers.RestrictedAreaLayer) importantAreaLayer.Draw(sb, mapSb, Camera);
            if(Settings.VisibleLayers.GribLayer) gribLayer.Draw(sb, mapSb, Camera);
            if(Settings.VisibleLayers.SeparationSchemeLayer) separationSchemeLayer.Draw(sb, mapSb, Camera);
            placenameLayer.Draw(sb, mapSb, Camera);
            buoyLayer.Draw(sb, mapSb, Camera);

            /*if (first)
                Camera.FocusOnPosition(MapGeo.BoundingPolygon[0].point);*/

            mapSb.End();

            first = false;
        }

        private void CorrectScaling()
        {
            // we first have to change back to the wrong scaling so that ScreenPosToWorldPos gives us the results we need
            Camera.Scale = new Vector3(Camera.Scale.Y, Camera.Scale.Y, 1);
            const int offset = 50;
            Vector2 screenCenter = new Vector2(Globals.Graphics.PreferredBackBufferWidth / 2,
                Globals.Graphics.PreferredBackBufferHeight / 2);
            float xDist = Utility.Utility.DistanceBetweenCoordinates(
                Camera.ScreenPosToWorldPos(screenCenter - new Vector2(offset, 0)),
                Camera.ScreenPosToWorldPos(screenCenter + new Vector2(offset, 0)));

            float yDist = Utility.Utility.DistanceBetweenCoordinates(
                Camera.ScreenPosToWorldPos(screenCenter - new Vector2(0, offset)),
                Camera.ScreenPosToWorldPos(screenCenter + new Vector2(0, offset)));

            float ratio = xDist / yDist;
            Camera.Scale = new Vector3(Camera.Scale.Y * ratio, Camera.Scale.Y, 1);
        }

        

        

        public void Load()
        {
            mapGeoLayer.LoadBoundingPoly(MapPack.OpenFile("boundary.poly"));

            foreach (SourceLayer sourceLayer in SourceLayers)
            {
                sourceLayer.LoadContent();
            }

            placenameLayer.LoadContent(MapPack);
            mapGeoLayer.LoadContent(MapPack);
            harbourLayer.LoadContent(MapPack);
            landmarkLayer.LoadContent(MapPack);
            navigationLineLayer.LoadContent(MapPack);
            importantAreaLayer.LoadContent(MapPack);
            gribLayer.LoadContent(MapPack);
            separationSchemeLayer.LoadContent(MapPack);
            buoyLayer.LoadContent(MapPack);

            Settings.LandmarkLayerSettings = landmarkLayer.LayerSettings;
            Settings.HarbourLayerSettings = harbourLayer.LayerSettings;
        }



        public class MapSettings
        {
            public VisibleLayersSetting VisibleLayers { get; set; } = new VisibleLayersSetting();

            public MapLayer.ILayerSettings HarbourLayerSettings;
            public MapLayer.ILayerSettings LandmarkLayerSettings;

            public class VisibleLayersSetting
            {
                public bool NavigationLineLayer = true;
                public bool MarinaLayer = true;
                public bool RestrictedAreaLayer = true;
                public bool LandmarkLayer = true;
                public bool GribLayer = true;
                public bool SeparationSchemeLayer = true;
            }
        }
    }
}
