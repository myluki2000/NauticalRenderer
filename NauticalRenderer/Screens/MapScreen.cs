using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Properties;
using Myra.Graphics2D.UI.Styles;
using NauticalRenderer.SlippyMap.Layers;
using NauticalRenderer.SlippyMap.UI;
using NauticalRenderer.UI;
using NauticalRenderer.Utility;
using OsmSharp.Tags;

namespace NauticalRenderer.Screens
{
    class MapScreen : Screen
    {
        private SpriteBatch spriteBatch;
        public readonly Desktop Desktop = new Desktop();
        private Label zoomLabel;
        private SlippyMap.SlippyMap slippyMap;

        /// <inheritdoc />
        public override void Initialize()
        {
            Stopwatch watch = Stopwatch.StartNew();

            slippyMap = new SlippyMap.SlippyMap(this);
            slippyMap.Load();

            watch.Stop();
            Console.WriteLine("Loaded map in " + watch.ElapsedMilliseconds + "ms.");

            spriteBatch = new SpriteBatch(Globals.Graphics.GraphicsDevice);

            InitUI();
        }

        /// <inheritdoc />
        public override void Draw()
        {
            spriteBatch.GraphicsDevice.Clear(Color.FromNonPremultiplied(30, 30, 30, 255));
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, null, null, null, null);
            slippyMap.Draw(spriteBatch);
            spriteBatch.End();
            Desktop.Render();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            slippyMap.Update(gameTime);
            zoomLabel.Text = "Zoom: " + slippyMap.Camera.Scale.Y;
        }

        public bool ShowOsmTagsWindow(string windowTitle, TagsCollectionBase tags)
        {
            if (Desktop.GetWindows().All(x => x.Content is OsmTagGrid grid && !ReferenceEquals(grid.TagsCollection, tags)))
            {
                Window window = new Window()
                {
                    Title = windowTitle,
                    MaxWidth = 400,
                };

                OsmTagGrid tagGrid = new OsmTagGrid { TagsCollection = tags };

                window.Content = tagGrid;
                window.ShowModal(Desktop);
                return true;
            }

            return false;
        }

        private void InitUI()
        {
            Grid grid = new Grid()
            {
                ShowGridLines = true,
                ColumnSpacing = 8,
                RowSpacing = 8
            };

            grid.ColumnsProportions.Add(new Proportion(ProportionType.Part, 0.3f));
            grid.ColumnsProportions.Add(Proportion.Fill);
            grid.ColumnsProportions.Add(Proportion.Auto);

            grid.RowsProportions.Add(Proportion.Auto);
            grid.RowsProportions.Add(Proportion.Fill);


            VerticalStackPanel propertiesPanel = new VerticalStackPanel()
            {
                GridColumn = 0,
                GridRow = 0,
                GridRowSpan = 2,
            };

            foreach (MapLayer.ILayerSettings setting in slippyMap.Settings.LayersSettings)
            {
                PropertyGrid propertyGrid = new PropertyGrid
                {
                    Object = setting,
                };

                propertiesPanel.Widgets.Add(propertyGrid);
            }



            grid.Widgets.Add(propertiesPanel);

            zoomLabel = new Label() { GridColumn = 2, GridRow = 1 };
            grid.Widgets.Add(zoomLabel);

            HorizontalStackPanel pnlControls = new HorizontalStackPanel()
            {
                Spacing = 8,
                HorizontalAlignment = HorizontalAlignment.Right,
                GridColumn = 2,
                GridRow = 0,
            };

            TextButton btnSettings = new TextButton() {Text = "Settings" };
            btnSettings.Click += (sender, args) =>
            {
                SettingsWindow mapSettingsWindow = new SettingsWindow(slippyMap);
                mapSettingsWindow.ShowModal(Desktop);
            };
            pnlControls.Widgets.Add(btnSettings);

            TextButton btnZoomOut = new TextButton() { Text = "  -  " };
            btnZoomOut.Click += (sender, args) =>
            {
                slippyMap.Camera.Scale /= new Vector3(1.1f, 1.1f, 1.0f);
            };
            pnlControls.Widgets.Add(btnZoomOut);

            TextButton btnZoomIn = new TextButton() { Text = "  +  " };
            btnZoomIn.Click += (sender, args) =>
            {
                slippyMap.Camera.Scale *= new Vector3(1.1f, 1.1f, 1.0f);
            };
            pnlControls.Widgets.Add(btnZoomIn);

            grid.Widgets.Add(pnlControls);

            Desktop.Root = grid;
        }
    }
}
