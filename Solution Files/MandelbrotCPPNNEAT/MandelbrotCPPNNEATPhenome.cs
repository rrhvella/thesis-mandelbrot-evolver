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
            Color.FromArgb(0, 255, 255),
            Color.FromArgb(0, 255, 0)
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

        public Bitmap GetImage(int ViewWidth, int ViewHeight, int iterationNumberLimit)
        {
            Bitmap fractalImage = new Bitmap(ViewWidth, ViewHeight);
            network.Reset();

            var drawingBuffer = fractalImage.LockBits(new Rectangle(0, 0, ViewWidth, ViewHeight),
                                                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            foreach (var x in Enumerable.Range(0, ViewWidth))
            {
                foreach (var y in Enumerable.Range(0, ViewHeight))
                {
                    var positionComplex = viewPosition + 
                                            (new Complex((double)x / ViewWidth, (double)y / ViewHeight) * viewScale);

                    var complex = Complex.Zero;
                    var currentMagnitude = complex.Magnitude;

                    int i = 0;

                    for (; i < iterationNumberLimit && currentMagnitude < ESCAPE_MAGNITUDE; i++)
                    {
                        complex = network.GetActivation(new Complex[] { positionComplex, complex });
                        currentMagnitude = complex.Magnitude;

                    }

                    Marshal.WriteInt32(drawingBuffer.Scan0 + drawingBuffer.Stride * y + x * BYTES_PER_INT, 
                                    ToColour(i, iterationNumberLimit));

                }
            }

            fractalImage.UnlockBits(drawingBuffer);

            return fractalImage;
        }

        private int ToColour(int iterationNumber, int iterationNumberLimit)
        {
            var normalisedIterationNumber = iterationNumber / (double)iterationNumberLimit;

            var brightness = 1 - normalisedIterationNumber;
            var baseColor = BASE_COLOURS[(int)Math.Round(normalisedIterationNumber * (BASE_COLOURS.Length - 1))];

            var r = (int)(baseColor.R * brightness);
            var g = (int)(baseColor.G * brightness);
            var b = (int)(baseColor.B * brightness);

            return 255 << ALPHA_OFFSET | r << RED_OFFSET | 
                    g << GREEN_OFFSET | b << BLUE_OFFSET;
        }

    }
}
