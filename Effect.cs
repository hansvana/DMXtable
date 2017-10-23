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
        public enum Effects { OFF, ODDEVEN, SINEWAVE, FILLUNFILL };
        public Effects currentEffect = Effects.OFF;
        private Action[] effectMethods;
        private Stopwatch stopwatch;
        private double fadeTime = 1000;
        long previousMilliseconds = 0;
        double msPerFrame;

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
                FXfillUnfill
            };
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public void update(double _msPerFrame)
        {
            msPerFrame = _msPerFrame;
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

        public int fadeSpeed
        {
            set { fadeTime = value; }
        }

        public double master
        {
            set { strengthMaster = value; }
        }

        public byte[] mix(byte[] original, int nthFixture, double mainMaster)
        {
            if (mixChannels == null)
                return original;

            if (onOff)
                return (strength[nthFixture] * strengthMaster > onOffTreshold ? mixChannels : original);

            byte[] result = new byte[Math.Min(original.Length, mixChannels.Length)];

            for (int i = 0; i < result.Length; i++)
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
            double sin = (Math.Sin((stopwatch.ElapsedMilliseconds / (fadeTime / Math.PI))) + 1) * .5;

            for (int i = 0; i < strength.Length; ++i)
            {
                strength[i] = (i % 2 == 0 ? sin : 1 - sin);
            }
        }

        private void FXsineWave()
        {
            double sin = (Math.Sin((stopwatch.ElapsedMilliseconds / (fadeTime / Math.PI))) + 1) * .5;
            double mod = 1.0 / strength.Length;
            for (int i = 0; i < strength.Length; ++i)
            {
                strength[i] = sin + (mod * i);
                if (strength[i] > 1.0) strength[i] = 2 - strength[i];
            }
        }

        private double FXfillUnfill_middle = .0;
        private double FXfillUnfill_dir = 1.0;
        private double FXfillUnfill_speed = .05; 

        private void FXfillUnfill()
        {
            FXfillUnfill_middle += (FXfillUnfill_dir * FXfillUnfill_speed);

            double delay, s;
            double half = ((strength.Length - 1) * .5);
            for (int i = 0; i < strength.Length; ++i)
            {
                delay = Math.Abs(i - half);
                s = FXfillUnfill_middle - (delay * FXfillUnfill_dir);

                if (i == 0)
                {
                    
                    if (s > 1) { s = 1; FXfillUnfill_dir = -1; }
                    if (s < 0) { s = 0; FXfillUnfill_dir = 1; }
                }

                strength[i] = Math.Min(Math.Max(0, s), 1);
            }

            FXfillUnfill_speed = (half / (fadeTime / (double)msPerFrame))*4;
        }
    }
}
