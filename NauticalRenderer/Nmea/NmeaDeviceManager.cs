using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using NmeaParser;

namespace NauticalRenderer.Nmea
{
    static class NmeaDeviceManager
    {
        public static IReadOnlyDictionary<string, (NmeaDeviceInfo deviceInfo, NmeaDevice device)> ConnectedDevices 
        {
            get
            {
                if(devices == null)
                    ConnectDevicesFromSettings();

                return devices;
            }
        }

        public static event Action ConnectedDevicesChanged;

        public static event EventHandler<NmeaMessageReceivedEventArgs> MessageReceived
        {
            add
            {
                foreach ((NmeaDeviceInfo deviceInfo, NmeaDevice device) in devices.Values)
                {
                    device.MessageReceived += value;
                }
            }

            remove
            {
                foreach ((NmeaDeviceInfo deviceInfo, NmeaDevice device) in devices.Values)
                {
                    device.MessageReceived -= value;
                }
            }
        }

        private static Dictionary<string, (NmeaDeviceInfo deviceInfo, NmeaDevice device)> devices = null;

        public static void ConnectDevicesFromSettings()
        {
            devices = new Dictionary<string, (NmeaDeviceInfo deviceInfo, NmeaDevice device)>();

            String s = (string) Globals.SettingsManager.GetSettingsValue("NmeaDevices");
            if(!string.IsNullOrEmpty(s))
                foreach (NmeaDeviceInfo devInfo in JsonSerializer.Deserialize<NmeaDeviceInfo[]>(s))
                {
                    ConnectDevice(devInfo);
                }
        }

        public static void ConnectDevice(NmeaDeviceInfo devInfo)
        {
            ConnectDevice(devInfo.PortName, devInfo.BaudRate);
        }

        public static void ConnectDevice(string portName, int baudRate)
        {
            // abort if device is already connected
            if (devices != null && ConnectedDevices.ContainsKey(portName)) return;

            SerialPort port = new SerialPort(portName, baudRate);
            SerialPortDevice dev = new SerialPortDevice(port);
            dev.OpenAsync();
            devices.Add(portName, (new NmeaDeviceInfo(portName, baudRate), dev));
            
            // Serialize device infos and save them in the settings
            NmeaDeviceInfo[] dis = devices.Select(x => x.Value.deviceInfo).ToArray();
            
            Globals.SettingsManager.SetSettingsValue("NmeaDevices", JsonSerializer.Serialize(dis));

            ConnectedDevicesChanged?.Invoke();
        }

        public static void DisconnectDevice(string portName)
        {
            (NmeaDeviceInfo deviceInfo, NmeaDevice device) = devices[portName];
            if (device.IsOpen) device.CloseAsync();
            device.Dispose();
            devices.Remove(portName);
            ConnectedDevicesChanged?.Invoke();
        }
    }
}
