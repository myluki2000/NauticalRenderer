using System;
using System.Collections.Generic;
using System.Text;
using Myra.Graphics2D.UI;
using NauticalRenderer.SlippyMap.Layers;
using NauticalRenderer.UI.Settings.Panels;

namespace NauticalRenderer.UI.Settings
{
    class SettingsWindow : Window
    {
        private readonly HorizontalStackPanel pnlMain = new HorizontalStackPanel() {
            Spacing = 16,
            ShowGridLines = true,
        };

        private readonly Tree tree = new Tree()
        {
            HasRoot = false,
            Width = 200,
        };

        public SettingsWindow(SlippyMap.SlippyMap slippyMap)
        {
            Width = 700;
            Height = 600;

            tree.SelectionChanged += Tree_SelectionChanged;
            pnlMain.Widgets.Add(tree);
            // add empty panel so the grid line shows
            pnlMain.Widgets.Add(new Panel());
            Content = pnlMain;

            TreeNode nmeaNode = tree.AddSubNode("NMEA");
            nmeaNode.AddSubNode("Connected Devices").Tag = typeof(NmeaSettingsPanel);
            nmeaNode.AddSubNode("GPS").Tag = typeof(GpsSettingsPanel);

            /*foreach (MapLayer layer in slippyMap.MapLayers)
            {
                TreeNode node = tree.AddSubNode(layer.GetType().FullName);
            }*/



        }

        private void Tree_SelectionChanged(object sender, EventArgs eventArgs)
        {
            if(pnlMain.Widgets.Count > 1)
                pnlMain.Widgets.RemoveAt(1);

            if (tree.SelectedRow.Tag != null)
                pnlMain.Widgets.Add((SettingsPanel) Activator.CreateInstance((Type) tree.SelectedRow.Tag));
            else
                pnlMain.Widgets.Add(new Panel()); // add empty panel so the grid line shows
        }
    }
}
