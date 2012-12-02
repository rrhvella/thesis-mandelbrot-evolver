using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NEATSpacesLibrary.GeneticAlgorithms;
using System.Drawing;
using NEATSpacesLibrary.CPPNNEAT;
using System.Numerics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace NEATSpacesLibrary.NEATSpaces
{
    public class GAPanel: Panel
    {
        public CPPNNEATGenome Genome
        {
            get;
            set;
        }

        public int Score
        {
            get;
            set;
        }

        public GAPanel(int zoomFactor)
        {
            this.zoomFactor = zoomFactor;

            Click += new EventHandler(GAPictureBox_Click);
            Paint += new PaintEventHandler(GAPanel_Paint);
        }


        private const int ESCAPE = 100;
        private const double MIN = -2;
        private const double MAX = 1;
        private int zoomFactor;

        private const double ESCAPE_MAGNITUDE = 2;

        private const int ALPHA_OFFSET = 24;
        private const int RED_OFFSET = 16;
        private const int GREEN_OFFSET = 8;
        private const int BLUE_OFFSET = 0;

        private const int BYTES_PER_INT = 4;

        private const double ARED = 0.019;
        private const double AGREEN = 0.015;
        private const double ABLUE = 0.011;

        private const double BRED = 0.8;
        private const double BGREEN = 0.8;
        private const double BBLUE = 0.8;

        void GAPanel_Paint(object sender, PaintEventArgs e)
        {
            if (Genome != null && Genome.Phenome != null)
            {
                var width = Width / zoomFactor;
                var height = Height / zoomFactor;

                var image = new Bitmap(width, height);
                var network = Genome.Phenome;
                network.Reset();

                var drawingBuffer = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                foreach (var x in Enumerable.Range(0, width))
                {
                    foreach (var y in Enumerable.Range(0, height))
                    {
                        var positionComplex = new Complex(MIN + (double)x / width * (MAX - MIN),
                                                                MIN + (double)y / height * (MAX - MIN));

                        var complex = Complex.Zero;
                        var currentMagnitude = complex.Magnitude; 

                        int i = 0;

                        for(; i < ESCAPE && currentMagnitude < ESCAPE_MAGNITUDE; i++) { 
                            complex = network.GetActivation(new Complex[] { positionComplex, complex });
                            currentMagnitude = complex.Magnitude; 

                        }

                        Marshal.WriteInt32(drawingBuffer.Scan0 + drawingBuffer.Stride * y + x * BYTES_PER_INT, ToColour(i));

                    }
                }

                image.UnlockBits(drawingBuffer);
                e.Graphics.DrawImage(image, ClientRectangle);
            }
        }

        private int ToColour(int iterationNumber)
        {
            var r = 127 + (int)(128 * Math.Cos(ARED * iterationNumber + BRED));
            var g = 127 + (int)(128 * Math.Cos(AGREEN * iterationNumber + BGREEN));
            var b = 127 + (int)(128 * Math.Cos(ABLUE * iterationNumber + BBLUE));

            return 255 << ALPHA_OFFSET | r << RED_OFFSET | 
                    g << GREEN_OFFSET | b << BLUE_OFFSET;
        }

        private void GAPictureBox_Click(object sender, EventArgs e)
        {
            Score++;
        }
    }
}
