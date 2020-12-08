using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using Myra.Graphics2D.UI;
using NauticalRenderer.Nmea;
using NmeaParser;

namespace NauticalRenderer.UI.Settings.Panels
{
    class NmeaSettingsPanel : SettingsPanel
    {
        private readonly ListBox lbConnectedDevices = new ListBox();
        private readonly TextButton btnAddDevice = new TextButton()
        {
            Text = "Add Device",
        };
        private readonly TextButton btnRemoveDevice = new TextButton() {
            Text = "Remove Device",
        };

        public NmeaSettingsPanel()
        {
            Spacing = 8;

            Widgets.Add(lbConnectedDevices);

            btnAddDevice.Click += BtnAddDevice_Clicked;
            btnRemoveDevice.Click += BtnRemoveDevice_Clicked;

            HorizontalStackPanel pnlListButtons = new HorizontalStackPanel()
            {
                Spacing = 8,
            };
            pnlListButtons.Widgets.Add(btnAddDevice);
            pnlListButtons.Widgets.Add(btnRemoveDevice);
            Widgets.Add(pnlListButtons);

            PopulateConnectedDevices();

            NmeaDeviceManager.ConnectedDevicesChanged += PopulateConnectedDevices;
        }

        private void PopulateConnectedDevices()
        {
            lbConnectedDevices.Items.Clear();

            foreach ((string port, (NmeaDeviceInfo deviceInfo, NmeaDevice device)) in NmeaDeviceManager.ConnectedDevices)
            {
                ListItem item = new ListItem(port);
                item.Tag = (deviceInfo, device);
                lbConnectedDevices.Items.Add(item);
            }
        }

        private void BtnAddDevice_Clicked(object sender, EventArgs eventArgs)
        {
            VerticalStackPanel pnl = new VerticalStackPanel()
            {
                Spacing = 8,
            };
            Dialog addDeviceDialog = new Dialog()
            {
                Title = "Add NMEA Device",
                Content = pnl,
            };

            ListBox lbSerialPorts = new ListBox();
            foreach (string port in SerialPort.GetPortNames())
            {
                lbSerialPorts.Items.Add(new ListItem(port));
            }
            pnl.Widgets.Add(lbSerialPorts);

            TextBox tbBaudRate = new TextBox()
            {
                Text = "4800",
            };

            pnl.Widgets.Add(tbBaudRate);

            addDeviceDialog.Closed += (o, args) =>
            {
                if (addDeviceDialog.Result)
                    if (int.TryParse(tbBaudRate.Text, out int baudRate))
                        NmeaDeviceManager.ConnectDevice(lbSerialPorts.SelectedItem.Text, baudRate);
            };
            addDeviceDialog.ShowModal(Desktop);
            
            
                
        }

        private void BtnRemoveDevice_Clicked(object sender, EventArgs eventArgs)
        {
            NmeaDeviceManager.DisconnectDevice(((ValueTuple<string, NmeaDevice>)lbConnectedDevices.Tag).Item1);
        }
    }
}
