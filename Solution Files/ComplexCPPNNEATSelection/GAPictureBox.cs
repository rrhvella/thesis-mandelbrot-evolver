using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NEATSpacesLibrary.GeneticAlgorithms;
using System.Drawing;
using NEATSpacesLibrary.CPPNNEAT;
using System.Numerics;

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


        private const int ESCAPE = 10;
        private int zoomFactor;

        void GAPanel_Paint(object sender, PaintEventArgs e)
        {
            if (Genome != null && Genome.Phenome != null)
            {
                var width = Width / zoomFactor;
                var height = Height / zoomFactor;

                var image = new Bitmap(width, height);
                var network = Genome.Phenome;
                network.Reset();

                var graphics = e.Graphics;
                var intensities = new double[width, height];

                var maxMagnitude = 0.0;

                foreach (var x in Enumerable.Range(0, width))
                {
                    foreach (var y in Enumerable.Range(0, height))
                    {
                        var positionComplex = new Complex((double)x / width,
                                                                (double)y / height);

                        var complex = Complex.Zero;
                        var currentMagnitude = complex.Magnitude; 

                        int i = 0;

                        for(; i < ESCAPE && currentMagnitude < 4; i++) { 
                            complex = network.GetActivation(new Complex[] { positionComplex, complex });
                            currentMagnitude = complex.Magnitude; 
                        }

                        var intensity = (int)((double)i / ESCAPE * 255);
                        //var intensity = currentMagnitude; 

                        if (!(double.IsInfinity(intensity) || double.IsNaN(intensity)) && 
                            intensity > maxMagnitude)
                        {
                            maxMagnitude = intensity;
                        }


                        intensities[x, y] = intensity;
                    }
                }

                foreach (var x in Enumerable.Range(0, width))
                {
                    foreach (var y in Enumerable.Range(0, height))
                    {
                        var intensity = intensities[x, y];
                        if (double.IsInfinity(intensity))
                        {
                            intensity = maxMagnitude;
                        }
                        else if(double.IsNaN(intensity))
                        {
                            intensity = 0;
                        }

                        var shadow = 0;

                        if (maxMagnitude > 0)
                        {
                            shadow = (int)(255 * intensity / maxMagnitude);
                        }

                        image.SetPixel(x, y, Color.FromArgb(shadow, shadow, shadow));
                    }
                }

                graphics.DrawImage(image, new RectangleF(0, 0, Width, Height));
            }
        }

        private void GAPictureBox_Click(object sender, EventArgs e)
        {
            Score++;
        }
    }
}
