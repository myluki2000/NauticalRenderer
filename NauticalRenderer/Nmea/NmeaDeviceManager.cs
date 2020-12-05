using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Text;
using NmeaParser;

namespace NauticalRenderer.Nmea
{
    static class NmeaDeviceManager
    {
        public static IReadOnlyDictionary<string, NmeaDevice> ConnectedDevices => devices;

        public static event Action ConnectedDevicesChanged;

        public static event EventHandler<NmeaMessageReceivedEventArgs> MessageReceived
        {
            add
            {
                foreach (NmeaDevice dev in devices.Values)
                {
                    dev.MessageReceived += value;
                }
            }

            remove
            {
                foreach (NmeaDevice dev in devices.Values)
                {
                    dev.MessageReceived -= value;
                }
            }
        }

        private static Dictionary<string, NmeaDevice> devices = new Dictionary<string, NmeaDevice>();

        public static void ConnectDevice(string portName, int baudRate)
        {
            /*SerialPort port = new SerialPort(portName, baudRate);
            devices.Add(portName, new SerialPortDevice(port));
            ConnectedDevicesChanged?.Invoke();*/
        }

        public static void DisconnectDevice(string portName)
        {
            NmeaDevice dev = devices[portName];
            if (dev.IsOpen) dev.CloseAsync();
            dev.Dispose();
            devices.Remove(portName);
            ConnectedDevicesChanged?.Invoke();
        }
    }
}
