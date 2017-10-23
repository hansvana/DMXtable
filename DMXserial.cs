using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;

namespace DMXtable
{
    class DMXserial
    {
        SerialPort port;
        byte[] message;

        public bool connect(string portname)
        {
            port = new SerialPort(portname, 115200);

            try
            {
                port.Open();
                return true;
                //byte[] byteArray = { 0x7E, 0x06, 0x08, 0x00, 0x00, 0xff, 0x00, 0xff, 0x00, 0x00, 0x00, 0x00, 0xE7 };
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
            }

            return false;
        }

        public bool send(byte[] byteArray)
        {
            if (port == null || !port.IsOpen)
                return false;

            int len = byteArray.Length;

            if (len > 0)
            {
                message = new byte[len+6];
                message[0] = 0x7E;
                message[1] = 0x06;
                message[2] = (byte)((len+1) & 0xFF);
                message[3] = (byte)((len >> 8) & 0xFF);
                message[4] = 0x00;
                byteArray.CopyTo(message, 5);
                message[len+5] = 0xE7;

                port.Write(message, 0, message.Length);
                //Console.WriteLine(String.Join(", ", message));
            }                

            return true;
        }

        public string[] ports()
        {
            return SerialPort.GetPortNames();
        }

        public bool isConnected
        {
            get { return port.IsOpen; }
        }
    }
}
