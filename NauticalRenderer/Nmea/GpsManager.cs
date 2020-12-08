using System;
using NmeaParser;
using NmeaParser.Messages;

namespace NauticalRenderer.Nmea
{
    public static class GpsManager
    {
        private static NmeaDevice _gpsDevice;

        private static NmeaDevice GpsDevice
        {
            get => _gpsDevice;
            set
            {
                if(_gpsDevice != null)
                    _gpsDevice.MessageReceived -= OnNmeaDataReceived;
                _gpsDevice = value;
                if(_gpsDevice != null)
                    _gpsDevice.MessageReceived += OnNmeaDataReceived;
            }
        }

        public static event EventHandler<Rmc> GpsDataReceived;

        public static void Initialize()
        {
            string portName = (string)Globals.SettingsManager.GetSettingsValue("GpsDevice");

            UpdateConnectedGpsDevice();
            NmeaDeviceManager.ConnectedDevicesChanged += UpdateConnectedGpsDevice;
        }

        private static void UpdateConnectedGpsDevice()
        {
            if (GpsDevice == null 
                && NmeaDeviceManager.ConnectedDevices.ContainsKey((string) Globals.SettingsManager.GetSettingsValue("GpsDevice")))
            {
                // gps device has been connected
                GpsDevice = NmeaDeviceManager.ConnectedDevices[(string)Globals.SettingsManager.GetSettingsValue("GpsDevice")].device;
            } else if(GpsDevice != null)
            {
                if (!NmeaDeviceManager.ConnectedDevices.ContainsKey(
                    (string) Globals.SettingsManager.GetSettingsValue("GpsDevice")))
                {
                    // gps device has been disconnected, so we should drop our reference to it
                    GpsDevice = null;
                }
                else
                {
                    // gps device setting might have changed, so just set gpsDevice to be sure
                    NmeaDevice newDevice = NmeaDeviceManager
                        .ConnectedDevices[(string) Globals.SettingsManager.GetSettingsValue("GpsDevice")].device;
                    GpsDevice = newDevice;
                }
            }

        }

        private static void OnNmeaDataReceived(object sender, NmeaMessageReceivedEventArgs args)
        {
            if(args.Message is Rmc rmc)
                GpsDataReceived?.Invoke(null, rmc);
        }
    }
}
