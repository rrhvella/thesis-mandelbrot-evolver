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

namespace ComplexCPPNNEATSelection
{
    public class FractalView: Panel
    {
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

        private int viewWidth;
        public int ViewWidth
        {
            get 
            { 
                return viewWidth; 
            }
            set
            {
                viewWidth = value;
                fractalImageCacheInvalidated = true;
            }
        }

        private int viewHeight;
        public int ViewHeight
        {
            get
            {
                return viewHeight;
            }
            set
            {
                viewHeight = value;
                fractalImageCacheInvalidated = true;
            }
        }

        private CPPNNEATGenome genome;
        public CPPNNEATGenome Genome
        {
            get
            {
                return genome;
            }
            set
            {
                fractalImageCacheInvalidated = true;
                genome = value;
            }
        }

        public int Score
        {
            get;
            set;
        }


        private Complex viewPosition;
        public Complex ViewPosition 
        {
            get
            {
                return viewPosition;
            }
            set
            {
                fractalImageCacheInvalidated = true;
                viewPosition = value;
            }
        }

        private double viewSize;
        public double ViewSize
        { 
            get 
            {
                return viewSize;
            } 
            set 
            {
                fractalImageCacheInvalidated = true;
                viewSize = value;
            } 
        }

        private int escape;
        public int Escape
        { 
            get 
            {
                return escape;
            } 
            set 
            {
                fractalImageCacheInvalidated = true;
                escape = value;
            } 
        }

        private Bitmap fractalImage;
        private bool fractalImageCacheInvalidated;
        public Image FractalImage
        {
            get
            {
                if (fractalImageCacheInvalidated)
                {
                    fractalImage = new Bitmap(ViewWidth, ViewHeight);

                    var network = Genome.Phenome;
                    network.Reset();

                    var drawingBuffer = fractalImage.LockBits(new Rectangle(0, 0, ViewWidth, ViewHeight),
                                                        ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                    foreach (var x in Enumerable.Range(0, ViewWidth))
                    {
                        foreach (var y in Enumerable.Range(0, ViewHeight))
                        {
                            var positionComplex = viewPosition + 
                                                    (new Complex((double)x / ViewWidth, (double)y / ViewHeight) * viewSize);

                            var complex = Complex.Zero;
                            var currentMagnitude = complex.Magnitude;

                            int i = 0;

                            for (; i < escape && currentMagnitude < ESCAPE_MAGNITUDE; i++)
                            {
                                complex = network.GetActivation(new Complex[] { positionComplex, complex });
                                currentMagnitude = complex.Magnitude;

                            }

                            Marshal.WriteInt32(drawingBuffer.Scan0 + drawingBuffer.Stride * y + x * BYTES_PER_INT, ToColour(i));

                        }
                    }

                    fractalImage.UnlockBits(drawingBuffer);
                    fractalImageCacheInvalidated = false;
                }

                return fractalImage;
            }
        }

        public event EventHandler<EventArgs> Selected;

        public FractalView()
        {
            fractalImageCacheInvalidated = true;

            MouseClick += new MouseEventHandler(GAPictureBox_Click);
            Paint += new PaintEventHandler(GAPanel_Paint);
        }


        void GAPanel_Paint(object sender, PaintEventArgs e)
        {
            if (Genome != null && Genome.Phenome != null)
            {
                e.Graphics.DrawImage(FractalImage, ClientRectangle);
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

        private void GAPictureBox_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Score++;
            }
            else if (e.Button == MouseButtons.Right)
            {
                Selected(this, new EventArgs());
            }
        }
    }
}
