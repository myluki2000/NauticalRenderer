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

        public (NmeaDeviceInfo deviceInfo, NmeaDevice device) SelectedDevice { get; private set; }

        public NmeaDeviceSelectionDialog()
        {
            Title = "NMEA Device Selection";
            Content = lbDevices;
            
            foreach ((string port, (NmeaDeviceInfo deviceInfo, NmeaDevice device)) in NmeaDeviceManager.ConnectedDevices)
            {
                ListItem item = new ListItem(port)
                {
                    Tag = (deviceInfo, device)
                };
                lbDevices.Items.Add(item);
            }

            lbDevices.SelectedIndexChanged += (sender, args) =>
            {
                SelectedDevice = (ValueTuple<NmeaDeviceInfo, NmeaDevice>)lbDevices.SelectedItem.Tag;
            };
        }
    }
}
