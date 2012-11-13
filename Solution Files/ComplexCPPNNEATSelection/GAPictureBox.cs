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
                var image = new Bitmap(Width / zoomFactor, Height / zoomFactor);
                var network = Genome.Phenome;
                network.Reset();

                var graphics = e.Graphics;

                foreach (var x in Enumerable.Range(0, image.Width))
                {
                    foreach (var y in Enumerable.Range(0, image.Height))
                    {
                        var complex = network.GetActivation(new Complex[] { new Complex((double)x / image.Width, 
                                                                                (double)y / image.Height)});
                        var currentMagnitude = complex.Magnitude; 
                        int i = 0;

                        for(; i < ESCAPE && currentMagnitude < 4; i++) { 
                            complex = network.GetActivation(new Complex[] { complex });
                            currentMagnitude = complex.Magnitude; 
                        }

                        var intensity = (int)((double)i / ESCAPE * 255);
                        image.SetPixel(x, y, Color.FromArgb(intensity, intensity, intensity));
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
