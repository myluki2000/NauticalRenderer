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

namespace NauticalRenderer.Screens
{
    class MapScreen : Screen
    {
        private SpriteBatch spriteBatch;
        private readonly Desktop desktop = new Desktop();
        private Label zoomLabel;
        private TextButton zoomOutButton = new TextButton() { Text = "  -  " };
        private TextButton zoomInButton = new TextButton() { Text = "  +  "};
        private SlippyMap.SlippyMap slippyMap;

        /// <inheritdoc />
        public override void Initialize()
        {
            Stopwatch watch = Stopwatch.StartNew();

            slippyMap = new SlippyMap.SlippyMap();
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
            desktop.Render();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            slippyMap.Update(gameTime);
            zoomLabel.Text = "Zoom: " + slippyMap.Camera.Scale.Y;
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
            grid.RowsProportions.Add(Proportion.Auto);
            grid.RowsProportions.Add(Proportion.Fill);

            /*grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

            CheckBox cbMarinaLayer = new CheckBox() { Text = "Marinas" };
            cbMarinaLayer.PressedChanged += (s, e) => { slippyMap.Layers.MarinaLayer.Visible = cbMarinaLayer.IsPressed; };
            grid.Widgets.Add(cbMarinaLayer);

            CheckBox cbNavigationLineLayer = new CheckBox() { Text = "Navigation Lines" };
            cbNavigationLineLayer.PressedChanged += (s, e) => { slippyMap.Layers.NavigationLineLayer.Visible = cbNavigationLineLayer.IsPressed; };
            grid.Widgets.Add(cbNavigationLineLayer);

            desktop.Root = grid;*/


            PropertyGrid propertyGrid = new PropertyGrid
            {
                Object = slippyMap.Settings,
                GridColumn = 0,
                GridRow = 0,
                GridRowSpan = 3,
                
            };

            grid.Widgets.Add(propertyGrid);

            zoomLabel = new Label() { GridColumn = 2, GridRow = 2};
            grid.Widgets.Add(zoomLabel);

            zoomOutButton.HorizontalAlignment = HorizontalAlignment.Right;
            zoomOutButton.GridColumn = 2;
            zoomOutButton.GridRow = 0;
            zoomOutButton.Click += (sender, args) =>
            {
                slippyMap.Camera.Scale /= new Vector3(1.1f, 1.1f, 1.0f);
            };
            grid.Widgets.Add(zoomOutButton);

            zoomInButton.HorizontalAlignment = HorizontalAlignment.Right;
            zoomInButton.GridColumn = 2;
            zoomInButton.GridRow = 1;
            zoomInButton.Click += (sender, args) =>
            {
                slippyMap.Camera.Scale *= new Vector3(1.1f, 1.1f, 1.0f);
            };
            grid.Widgets.Add(zoomInButton);

            desktop.Root = grid;
        }
    }
}
