using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using NauticalRenderer.Data;
using NauticalRenderer.Data.MapPack;
using NauticalRenderer.Screens;
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
        public IReadOnlyList<MapLayer> MapLayers;

        private readonly MapScreen mapScreen;


        public SlippyMap(MapScreen mapScreen)
        {
            this.mapScreen = mapScreen;

            MapLayers = new List<MapLayer>()
            {
                new MapGeoLayer(),
                new LanduseLayer(),
                new StreetLayer(),
                new FacilitiesLayer(),
                new HarbourLayer(mapScreen),
                new NavigationLineLayer(),
                new ImportantAreaLayer(mapScreen),
                new SeparationSchemeLayer(),
                new DangerSymbolsLayer(),
                new BuoyLayer(),
                new LandmarkLayer(),
                new GribLayer(),
                new PlacenameLayer(),
                new GpsLayer(),
            };

            Camera.TranslationChanged += (sender, e) => { CorrectScaling(); };
            MapPack = new MapPack("Content/German-North-Sea.mappack");
        }

        
        public void Update(GameTime gameTime)
        {
            Camera.Update();

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

            if(touchState.Count == 2 && lastTouchState.Count == 2)
            {
                float distNow = (touchState[0].Position - touchState[1].Position).Length();
                float distLast = (lastTouchState[0].Position - lastTouchState[1].Position).Length();

                float s = distNow / distLast;

                Camera.Scale *= new Vector3(s, s, 1);
            }

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
            if (mapScreen.Desktop.IsMouseOverGUI) return;

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
                mouseInertia = (Camera.MousePosition - Camera.ScreenPosToWorldPos(lastMouseState.Position.ToVector2()))
                          / (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (Math.Abs(mouseInertia.X) + Math.Abs(mouseInertia.Y) < 0.001f)
            {
                mouseInertia = Vector2.Zero;
            }

            if (mouseIsDragging)
            {
                Vector2 delta = (mouseState.Position - lastMouseState.Position).ToVector2();
                Camera.Translation += new Vector3(delta, 0) / Camera.Scale;
            }
            else if (mouseInertia != Vector2.Zero)
            {
                Camera.Translation += new Vector3(mouseInertia * (float)gameTime.ElapsedGameTime.TotalSeconds, 0);
                mouseInertia *= 0.85f;
            }

            lastMouseState = mouseState;
        }

        public void Draw(SpriteBatch sb)
        {
            Camera.Update();

            foreach (SourceLayer sourceLayer in SourceLayers)
            {
                sourceLayer.Draw(Camera);
            }

            SpriteBatch mapSb = new SpriteBatch(Globals.Graphics.GraphicsDevice);

            mapSb.Begin(transformMatrix: Camera.GetMatrix());

            foreach (MapLayer mapLayer in MapLayers)
            {
                mapLayer.Draw(sb, mapSb, Camera);
            }
            

            mapSb.End();
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
            foreach (SourceLayer sourceLayer in SourceLayers)
            {
                sourceLayer.LoadContent();
            }

            foreach (MapLayer mapLayer in MapLayers)
            {
                mapLayer.LoadContent(MapPack);
            }

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();

            Settings.LayersSettings = MapLayers.Select(x => x.LayerSettings).ToList();
        }



        public class MapSettings
        {
            public List<MapLayer.ILayerSettings> LayersSettings;
        }
    }
}
