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

            int len = 255;

            if (len > 0)
            {
                message = new byte[255];
                message[0] = 0x7E;
                message[1] = 0x06;
                //message[2] = (byte)(len+5) & 0xFF;
                message[2] = 0xFF;
                message[3] = 0x00;
                message[4] = 0x00;
                byteArray.CopyTo(message, 5);
                message[len-1] = 0xE7;

                port.Write(message, 0, message.Length);
                //Console.WriteLine(String.Join(", ", message));
                //Console.WriteLine(message.Length + " bytes sent");
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
