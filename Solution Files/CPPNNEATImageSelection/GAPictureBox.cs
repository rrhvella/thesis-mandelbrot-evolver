using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NEATSpacesLibrary.GeneticAlgorithms;
using System.Drawing;
using NEATSpacesLibrary.CPPNNEAT;

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

        public GAPanel()
        {
            Click += new EventHandler(GAPictureBox_Click);
            Paint += new PaintEventHandler(GAPanel_Paint);
        }

        void GAPanel_Paint(object sender, PaintEventArgs e)
        {
            if (Genome != null && Genome.Phenome != null)
            {
                var image = new Bitmap(Width, Height);
                var network = Genome.Phenome;
                network.Reset();

                var graphics = e.Graphics;

                foreach (var x in Enumerable.Range(0, Width))
                {
                    foreach (var y in Enumerable.Range(0, Height))
                    {
                        var intensity = (int)(network.GetActivation(new double[] { (float)x / Width, 
                                                                                    (float)y / Height }) * 255);
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
