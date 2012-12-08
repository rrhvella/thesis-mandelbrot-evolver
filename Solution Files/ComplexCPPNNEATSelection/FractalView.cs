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
using MandelbrotCPPNNEAT;


namespace ComplexCPPNNEATSelection
{
    public class FractalView: Panel
    {
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

        private MandelbrotCPPNNEATGenome genome;
        public MandelbrotCPPNNEATGenome Genome
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
                    fractalImage = Genome.Phenome.GetImage(ViewWidth, ViewHeight, escape);
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
