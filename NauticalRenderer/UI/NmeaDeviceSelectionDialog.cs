using System;
using System.Collections.Generic;
using System.Text;
using Myra.Graphics2D.UI;
using NauticalRenderer.Nmea;
using NmeaParser;

namespace NauticalRenderer.UI
{
    class NmeaDeviceSelectionDialog : Dialog
    {
        ListBox lbDevices = new ListBox();

        public (string port, NmeaDevice device) SelectedDevice { get; private set; }

        public NmeaDeviceSelectionDialog()
        {
            Title = "NMEA Device Selection";
            Content = lbDevices;

            foreach ((string port, NmeaDevice device) in NmeaDeviceManager.ConnectedDevices)
            {
                ListItem item = new ListItem(port);
                item.Tag = (port, device);
                lbDevices.Items.Add(item);
            }

            lbDevices.SelectedIndexChanged += (sender, args) =>
            {
                SelectedDevice = (ValueTuple<string, NmeaDevice>)lbDevices.SelectedItem.Tag;
            };
        }
    }
}
