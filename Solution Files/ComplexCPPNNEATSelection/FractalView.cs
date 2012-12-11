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
    /// <summary>
    /// Draws the view of a Mandelbrot fractal in the complex plane, with the membership based on a CPPN-NEAT genome.
    /// </summary>
    public class FractalView: Panel
    {
        /// <summary>
        /// The number of iterations performed before a Network is judged to tend to infinity.
        /// </summary>
        private const int ESCAPE = 200;


        /// <summary>
        /// The resolution width, in pixels, of the fractal views. Resolution here refers to the number of complex numbers sampled 
        /// within a view.
        /// </summary>
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

        /// <summary>
        /// The resolution height, in pixels, of the fractal views. Resolution here refers to the number of complex numbers sampled 
        /// within a view.
        /// </summary>
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

        /// <summary>
        /// The genome on which the view is based.
        /// </summary>
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

        /// <summary>
        /// The score of the genome based on the number of user clicks.
        /// </summary>
        public int Score
        {
            get;
            set;
        }

        /// <summary>
        /// Is true if the image needs to be regenerated.
        /// </summary>
        private bool fractalImageCacheInvalidated;

        /// <summary>
        /// The image containing the rendered view.
        /// </summary>
        private Bitmap fractalImage;
        public Image FractalImage
        {
            get
            {
                if (fractalImageCacheInvalidated)
                {
                    fractalImage = Genome.Phenome.GetImage(ViewWidth, ViewHeight, ESCAPE);
                    fractalImageCacheInvalidated = false;
                }

                return fractalImage;
            }
        }

        /// <summary>
        /// Fired when the user selects this view. 
        /// </summary>
        public event EventHandler<EventArgs> Selected;

        public FractalView()
        {
            fractalImageCacheInvalidated = true;

            MouseClick += new MouseEventHandler(GAPictureBox_Click);
            Paint += new PaintEventHandler(GAPanel_Paint);
        }


        /// <summary>
        /// Handles the event fired when the control is painted.
        /// </summary>
        void GAPanel_Paint(object sender, PaintEventArgs e)
        {
            //If a genome is registered, draw it's corresponding image.
            if (Genome != null && Genome.Phenome != null)
            {
                e.Graphics.DrawImage(FractalImage, ClientRectangle);
            }
        }

        /// <summary>
        /// Handles the event fired when the control is clicked on.
        /// </summary>
        private void GAPictureBox_Click(object sender, MouseEventArgs e)
        {
            //Left button increases the score.
            if (e.Button == MouseButtons.Left)
            {
                Score++;
            }
            //Right button selects the view.
            else if (e.Button == MouseButtons.Right)
            {
                Selected(this, new EventArgs());
            }
        }
    }
}
