using System;
using System.Drawing;
using System.Windows.Forms;
using MandelbrotCPPNNEAT;

namespace ComplexCPPNNEATSelection
{
    public class FractalView : Panel
    {
        private const int ESCAPE = 200;

        private int viewResolutionWidth;

        public int ViewResolutionWidth
        {
            get
            {
                return viewResolutionWidth;
            }
            set
            {
                viewResolutionWidth = value;
                fractalImageCacheInvalidated = true;
            }
        }

        private int viewResolutionHeight;

        public int ViewResolutionHeight
        {
            get
            {
                return viewResolutionHeight;
            }
            set
            {
                viewResolutionHeight = value;
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

        private bool fractalImageCacheInvalidated;

        private Bitmap fractalImage;

        public Image FractalImage
        {
            get
            {
                if (fractalImageCacheInvalidated && Genome != null)
                {
                    fractalImage = Genome.Phenome.GetImage(ViewResolutionWidth, ViewResolutionHeight, ESCAPE);
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

        private void GAPanel_Paint(object sender, PaintEventArgs e)
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