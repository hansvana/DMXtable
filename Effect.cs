using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DMXtable
{
    class Effect
    {
        private byte[] mixChannels;
        private double[] strength;
        private double strengthMaster = 0;
        public bool onOff = false;
        public double onOffTreshold = 0.5;
        public enum Effects { OFF, ODDEVEN, SINEWAVE, INOUT };
        public Effects currentEffect = Effects.OFF;
        private Action[] effectMethods;
        private Stopwatch stopwatch;
        private double fadeTime = 0;

        public double strengthExample
        {
            get { return strength[0]; }
        }

        public Effect(int gridSize)
        {
            strength = new double[gridSize];
            effectMethods = new Action[]
            {
                FXoff,
                FXoddEven,
                FXsineWave,
                FXinOut
            };
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public void update(double fadeSpeed)
        {
            fadeTime = (double)fadeSpeed;
            effectMethods[(int)currentEffect].Invoke();
        }

        public int size
        {
            set { strength = new double[value]; }
        }

        public byte[] channels
        {
            set { mixChannels = value; }
        }

        public double master
        {
            set { strengthMaster = value; }
        }

        public byte[] mix (byte[] original, int nthFixture, double mainMaster)
        {
            if (mixChannels == null)
                return original;

            if (onOff)
                return (strength[nthFixture] * strengthMaster > onOffTreshold ? mixChannels : original);

            byte[] result = new byte[Math.Min(original.Length, mixChannels.Length)];

            for (int i = 0; i < result.Length; i++ )
            {
                result[i] = toClampedByte(
                    (original[i] + 
                        (
                            (mixChannels[i] - original[i]) * 
                            (strength[nthFixture] * strengthMaster)
                        )
                    )
                    * mainMaster                    
                );
            }

            return result;
        }

        public byte toClampedByte(double n)
        {
            n = Math.Max(0, n);
            n = Math.Min(n, 255);
            return (byte)Convert.ToInt32(n);
        }

        private void FXoff()
        {
            for (int i = 0; i < strength.Length; ++i)
            {
                strength[i] = 0;
            }
        }

        private void FXoddEven()
        {
            double sin = (Math.Sin((stopwatch.ElapsedMilliseconds/(fadeTime/Math.PI))) + 1) * .5;

            for (int i = 0; i < strength.Length; ++i)
            {
                strength[i] = (i%2 == 0 ? sin : 1 - sin);
            }
        }

        private void FXsineWave()
        {
            double sin = (Math.Sin((stopwatch.ElapsedMilliseconds / (fadeTime / Math.PI))) + 1) * .5;
            double mod = 1.0 / strength.Length;
            for (int i = 0; i < strength.Length; ++i)
            {
                strength[i] = sin + (mod*i);
                if (strength[i] > 1.0) strength[i] = 2 - strength[i];
            }
        }

        private void FXinOut()
        {

        }

    }
}
