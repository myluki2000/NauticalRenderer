using System;
using System.Collections.Generic;
using System.Text;
using Myra.Graphics2D.UI;
using NauticalRenderer.SlippyMap.Layers;

namespace NauticalRenderer.UI
{
    class SettingsWindow : Window
    {
        private readonly HorizontalStackPanel pnlMain = new HorizontalStackPanel() {
            Spacing = 8,
        };
        private readonly Tree tree = new Tree()
        {
            HasRoot = false,
            Width = 500,
        };
        public SettingsWindow(SlippyMap.SlippyMap slippyMap)
        {
            Content = pnlMain;

            foreach (MapLayer layer in slippyMap.MapLayers)
            {
                TreeNode node = tree.AddSubNode(layer.GetType().FullName);
            }
            pnlMain.Widgets.Add(tree);


        }
    }
}
