using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using NEATSpacesLibrary.CPPNNEAT;
using System.Numerics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MandelbrotCPPNNEAT
{
    public class MandelbrotCPPNNEATPhenome 
    {
        private const double ESCAPE_MAGNITUDE = 2;

        private const int ALPHA_OFFSET = 24;
        private const int RED_OFFSET = 16;
        private const int GREEN_OFFSET = 8;
        private const int BLUE_OFFSET = 0;

        private const int BYTES_PER_INT = 4;

        private static readonly Color[] BASE_COLOURS = new Color[] { 
            Color.FromArgb(255, 221, 0),
            Color.FromArgb(161, 255, 0),
            Color.FromArgb(0, 33, 225),
            Color.FromArgb(93, 0, 225)
        };


        private CPPNNetwork network;
        private Complex viewPosition;
        private double viewScale;

        public MandelbrotCPPNNEATPhenome(CPPNNetwork viewNetwork, Complex viewPosition, double viewScale)
        {
            this.network = viewNetwork;
            this.viewPosition = viewPosition;
            this.viewScale = viewScale;
        }

        public Bitmap GetImage(int viewWidth, int viewHeight, int iterationNumberLimit)
        {
            Bitmap fractalImage = new Bitmap(viewWidth, viewHeight);

            var drawingBuffer = fractalImage.LockBits(new Rectangle(0, 0, viewWidth, viewHeight),
                                                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            foreach(var tup in GetIterationNumbers(viewWidth, viewHeight, iterationNumberLimit))
            {
                Marshal.WriteInt32(drawingBuffer.Scan0 + drawingBuffer.Stride * tup.Y + tup.X * BYTES_PER_INT, 
                                ToColour(tup.IterationNumber, iterationNumberLimit, tup.Z));
            }

            fractalImage.UnlockBits(drawingBuffer);

            return fractalImage;
        }

        public double GetVarianceBasedFitness(int viewWidth, int viewHeight, int iterationNumberLimit)
        {
            var setLessThanLimit = new List<int>();
            var averageLessThanLimit = 0.0;
            var totalAtLimit = 0;

            foreach(var tup in GetIterationNumbers(viewWidth, viewHeight, iterationNumberLimit))
            {
                if (tup.IterationNumber == iterationNumberLimit)
                {
                    totalAtLimit += 1;
                }
                else
                {
                    averageLessThanLimit += tup.IterationNumber;
                    setLessThanLimit.Add(tup.IterationNumber);
                }
            }

            averageLessThanLimit /= setLessThanLimit.Count;

            var denominator = ((setLessThanLimit.Count - 1) * totalAtLimit);

            if(denominator == 0) 
            {
                return 0;
            }

            return setLessThanLimit.Select(i => Math.Pow(i - averageLessThanLimit, 2)).Sum() / 
                        denominator;
        }

        private struct IterationNumberTuple
        {
            public int X;
            public int Y;
            public int IterationNumber;
            public Complex Z;

            public IterationNumberTuple(int x, int y, int iterationNumber, Complex z)
            {
                this.X = x;
                this.Y = y;
                this.IterationNumber = iterationNumber;
                this.Z = z;
            }
        }

        private IEnumerable<IterationNumberTuple> GetIterationNumbers(int viewWidth, int viewHeight, 
                                                                    int iterationNumberLimit)
        {
            network.Reset();

            foreach (var x in Enumerable.Range(0, viewWidth))
            {
                foreach (var y in Enumerable.Range(0, viewHeight))
                {
                    var c = viewPosition + 
                                (new Complex((double)x / viewWidth, (double)y / viewHeight) * viewScale);

                    var z = Complex.Zero;
                    var currentMagnitude = z.Magnitude;

                    int i = 0;

                    for (; i < iterationNumberLimit && currentMagnitude < ESCAPE_MAGNITUDE; i++)
                    {
                        z = network.GetActivation(new Complex[] { z, c });
                        currentMagnitude = z.Magnitude;
                    }

                    yield return new IterationNumberTuple(x, y, i, z);
                }
            }
        }

        private int ToColour(int iterationNumber, int iterationNumberLimit, Complex z)
        {
            var shadingAdjustment = Math.Log(Math.Log(z.Magnitude), 2);
            var normalisedIterationNumber = iterationNumber / (double)iterationNumberLimit;

            var brightness = 1 - normalisedIterationNumber - shadingAdjustment / iterationNumberLimit;
            var baseColor = BASE_COLOURS[(int)Math.Round(normalisedIterationNumber * (BASE_COLOURS.Length - 1))];

            var r = (int)(baseColor.R * brightness);
            var g = (int)(baseColor.G * brightness);
            var b = (int)(baseColor.B * brightness);

            return 255 << ALPHA_OFFSET | r << RED_OFFSET | 
                    g << GREEN_OFFSET | b << BLUE_OFFSET;
        }

    }
}
