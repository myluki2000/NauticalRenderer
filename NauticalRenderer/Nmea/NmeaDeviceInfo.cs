using System;
using System.Collections.Generic;
using System.Text;

namespace NauticalRenderer.Nmea
{
    [Serializable]
    struct NmeaDeviceInfo
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }

        public NmeaDeviceInfo(string portName, int baudRate)
        {
            PortName = portName;
            BaudRate = baudRate;
        }
    }
}
