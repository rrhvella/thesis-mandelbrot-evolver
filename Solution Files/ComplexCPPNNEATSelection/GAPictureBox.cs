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


        private const int ESCAPE = 50;
        private int zoomFactor;

        void GAPanel_Paint(object sender, PaintEventArgs e)
        {
            if (Genome != null && Genome.Phenome != null)
            {
                var image = new Bitmap(Width, Height);
                var network = Genome.Phenome;
                network.Reset();

                var graphics = e.Graphics;
                var intensities = new double[Width, Height];

                var maxMagnitude = 0.0;

                foreach (var x in Enumerable.Range(0, Width))
                {
                    foreach (var y in Enumerable.Range(0, Height))
                    {
                        var intensity = network.GetActivation(new Complex[] { new Complex((double)x / Width, 
                                                                                    (double)y / Height)}).Magnitude;

                        if (double.IsInfinity(intensity) || double.IsNaN(intensity))
                        {
                            intensity = 0;
                        }

                        if (intensity > maxMagnitude)
                        {
                            maxMagnitude = intensity;
                        }


                        intensities[x, y] = intensity;
                    }
                }

                foreach (var x in Enumerable.Range(0, Width))
                {
                    foreach (var y in Enumerable.Range(0, Width))
                    {
                        var intensity = (int)(255 * intensities[x, y] / maxMagnitude);
                        image.SetPixel(x, y, Color.FromArgb(intensity, intensity, intensity));
                    }
                }
            
                graphics.DrawImage(image, 0, 0);
            }
        }

        private void GAPictureBox_Click(object sender, EventArgs e)
        {
            Score++;
        }
    }
}
