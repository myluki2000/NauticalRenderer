using System;
using Myra.Graphics2D.UI;

namespace NauticalRenderer.UI.Settings.Panels
{
    class GpsSettingsPanel : SettingsPanel
    {
        private string SelectedDevice
        {
            get => (string)Globals.SettingsManager.GetSettingsValue("GpsDevice");
            set
            {
                Globals.SettingsManager.SetSettingsValue("GpsDevice", value);
                lblSelectedDevice.Text = "Selected Device: " + value;
            }
        }

        private readonly TextButton btnSelectDevice = new TextButton() { Text = "Select Device" };
        private readonly Label lblSelectedDevice = new Label();
        /// <inheritdoc />
        public GpsSettingsPanel()
        {
            HorizontalStackPanel pnlDeviceSelect = new HorizontalStackPanel() { Spacing = 8 };
            pnlDeviceSelect.Widgets.Add(lblSelectedDevice);
            pnlDeviceSelect.Widgets.Add(btnSelectDevice);
            btnSelectDevice.Click += BtnSelectDevice_Clicked;

            lblSelectedDevice.Text = "Selected Device: " + SelectedDevice;

            Widgets.Add(pnlDeviceSelect);
        }

        private void BtnSelectDevice_Clicked(object sender, EventArgs eventArgs)
        {
            NmeaDeviceSelectionDialog deviceDialog = new NmeaDeviceSelectionDialog();
            deviceDialog.Closed += (o, args) =>
            {
                if (deviceDialog.Result)
                {
                    SelectedDevice = deviceDialog.SelectedDevice.deviceInfo.PortName;
                }
            };
            deviceDialog.ShowModal(Desktop);
        }
    }
}
