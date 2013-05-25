using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using CPPNNEAT.CPPNNEAT;

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

        public Bitmap GetImage(int viewResolutionWidth, int viewResolutionHeight, int iterationNumberLimit)
        {
            Bitmap fractalImage = new Bitmap(viewResolutionWidth, viewResolutionHeight);

            var drawingBuffer = fractalImage.LockBits(new Rectangle(0, 0, viewResolutionWidth, viewResolutionHeight),
                                                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            foreach (var tup in GetIterationNumbers(viewResolutionWidth, viewResolutionHeight, iterationNumberLimit))
            {
                Marshal.WriteInt32(drawingBuffer.Scan0 + drawingBuffer.Stride * tup.Y + tup.X * BYTES_PER_INT,
                ToColour(tup.IterationNumber, iterationNumberLimit, tup.Z));
            }

            fractalImage.UnlockBits(drawingBuffer);

            return fractalImage;
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

        private IEnumerable<IterationNumberTuple> GetIterationNumbers(int viewWidthResolution, int viewHeightResolution,
    int iterationNumberLimit)
        {
            network.Reset();

            foreach (var x in Enumerable.Range(0, viewWidthResolution))
            {
                foreach (var y in Enumerable.Range(0, viewHeightResolution))
                {
                    var c = viewPosition +
                                (new Complex((double)x / viewWidthResolution, (double)y / viewHeightResolution) * viewScale);

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
            var adjustedIterationNumber = iterationNumber + Math.Log(Math.Log(z.Magnitude)) / Math.Log(2);

            var brightness = 1 - adjustedIterationNumber / iterationNumberLimit;
            var baseColour = BASE_COLOURS[(int)Math.Round(iterationNumber / (double)iterationNumberLimit *
                                            (BASE_COLOURS.Length - 1))];

            var r = (int)(baseColour.R * brightness);
            var g = (int)(baseColour.G * brightness);
            var b = (int)(baseColour.B * brightness);

            return 255 << ALPHA_OFFSET | r << RED_OFFSET |
                    g << GREEN_OFFSET | b << BLUE_OFFSET;
        }
    }
}