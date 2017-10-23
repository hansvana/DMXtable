using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMXtable
{
    class Fixture
    {
        public string name { get; set; }
        public int nChannels { get; set; }
        public int startAddr { get; set; }
        public string currentValues { get { return String.Join(",", currentChannels); } }
        public string memberOfGroups {
            get {
                if (groups != null)
                    return String.Join(",", groups);
                else
                    return "";
            }
        }
        public byte[] currentChannels;
        private byte[] targetChannels;
        private double[] channelV;

        private bool isFading = false;
        private string[] groups;        

        public Fixture(string _name, int _nChannels, int _startAddr, string _groups = "")
        {
            name = _name;
            nChannels = _nChannels;
            startAddr = _startAddr;

            if (_groups != "")
                groups = _groups.Split(',');

            currentChannels = new byte[nChannels];
        }

        public void updateChannels(byte[] target, double fadeSpeed = 0)
        {
            if (target.Length < nChannels)
                Array.Resize<byte>(ref target, nChannels);

            if (fadeSpeed == 0 || channelDistance(currentChannels, target) < 10.0)
            {
                currentChannels = target;
            } else
            {
                channelV = new double[nChannels];
                for (int i = 0; i < nChannels; i++)
                {
                    channelV[i] = (target[i] - currentChannels[i]) / fadeSpeed;
                }

                isFading = true;
                targetChannels = target;
            }
        }

        public void fade()
        {
            if (!isFading)
                return;

            if (channelDistance(currentChannels, targetChannels) < 10.0)
            {
                currentChannels = targetChannels;
                isFading = false;
            } else
            {
                for (int i = 0; i < nChannels; i++)
                {
                    if (Math.Abs(targetChannels[i] - currentChannels[i]) < Math.Abs(channelV[i]))
                    {
                        channelV[i] = 0;
                        currentChannels[i] = targetChannels[i];
                    } else
                    {
                        currentChannels[i] = (byte)(currentChannels[i] + channelV[i]);
                    }
                }
            }            
        }

        public double channelDistance(byte[] origin, byte[] target)
        {
            double B, C = 0;
            for (int i = 0; i < origin.Length; i++)
            {
                B = (target[i] - origin[i]);
                C += B * B;
            }

            return Math.Sqrt(C);
        }

        public bool isInGroup(string name)
        {
            if (groups == null)
                return false;

            return (groups.Contains(name));
        }
    }
}
