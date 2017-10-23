using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DMXtable
{
    public class MyColor
    {
        private static readonly Dictionary<string, Color> dictionary =
            new Dictionary<string, Color>
            {
                { "Red", Color.FromRgb(255,0,0) },
                { "Orange", Color.FromRgb(255,127,0) },
                { "Yellow", Color.FromRgb(255,255,0) },
                { "Chartreuse", Color.FromRgb(127,255,0) },
                { "Lime", Color.FromRgb(0,255,0) },
                { "SpringGreen", Color.FromRgb(0,255,127) },
                { "Aqua", Color.FromRgb(0,255,255) },
                { "Azure", Color.FromRgb(0,127,255) },
                { "Blue", Color.FromRgb(0,0,255) },
                { "Violet", Color.FromRgb(127,0,255) },
                { "Fuchsia", Color.FromRgb(255,0,255) },
                { "Rose", Color.FromRgb(255,0,127) },
                { "White", Color.FromRgb(255,255,255) },
                { "Black", Color.FromRgb(0,0,0) },
            };

        private Color c;

        public MyColor()
        {
            c = dictionary["Black"];
        }

        public MyColor(string colorName)
        {
            if (dictionary.ContainsKey(colorName)) {
                c = dictionary[colorName];
            }
        }

        public MyColor(byte R, byte G, byte B)
        {
            c = Color.FromRgb(R, G, B);
        }

        public MyColor(byte b)
        {
            c = Color.FromRgb(b,b,b);
        }

        public MyColor(byte[] b)
        {
            if (b.Length >= 3)
                c = Color.FromRgb(b[0], b[1], b[2]);
        }

        public byte R
        {
            get { return c.R; }
            set { c.R = value; }
        }
        public byte G
        {
            get { return c.G; }
            set { c.G = value; }
        }
        public byte B
        {
            get { return c.B; }
            set { c.B = value; }
        }
        public string Hex
        {
            get
            {
                return "#FF" +
                    c.R.ToString("X2") +
                    c.G.ToString("X2") +
                    c.B.ToString("X2");
            }
        }

        public byte this[int i]
        {
            get { return new byte[] { c.R, c.G, c.B }[i]; }
            set {
                switch(i)
                {
                    case 0:  c.R = value; break;
                    case 1:  c.G = value; break;
                    case 2:  c.B = value; break;
                }
            }
        }

        public double distance(MyColor target)
        {
            double dR = c.R - target.R;
            double dG = c.G - target.G;
            double dB = c.B - target.B;

            return Math.Sqrt(dR * dR + dG * dG + dB * dB);
        }
    }
}
