using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Numerics;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace ComplexCPPNNEATSelection
{
    public partial class MainForm : Form
    {
        private int currentOutputIndex;

        public MainForm()
        {
            InitializeComponent();

            foreach (FractalView view in fractalSelectionInstance.Views)
            {
                view.MouseEnter += new EventHandler(view_MouseHover);
                view.Selected += new EventHandler<EventArgs>(view_Selected);
            }

            var imageIndex = Directory.GetFiles(".", "image-*")
                                        .Select(filename => Int32.Parse(Regex.Match(filename, ".*image-([0-9]*).*").Groups[1].Captures[0].Value))
                                        .ToList();

            currentOutputIndex = (imageIndex.Count == 0)? 0 : imageIndex.Max() + 1;
        }

        void view_Selected(object sender, EventArgs e)
        {
            finalView.Genome = (sender as FractalView).Genome;
            finalView.Refresh();
        }

        void view_MouseHover(object sender, EventArgs e)
        {
            genomeView.Text = (sender as FractalView).Genome.ToString();
        }

        public static void Main(string[] args)
        {
            Application.Run(new MainForm()); 
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                fractalSelectionInstance.NextGeneration();
                generations.Text = fractalSelectionInstance.NumberOfGenerations.ToString();
            }
        }

        private void Output_Click(object sender, EventArgs e)
        {
            finalView.FractalImage.Save(String.Format("mandelbrot-{0}-image.png", currentOutputIndex), ImageFormat.Png);

            StreamWriter writerNetwork = new StreamWriter(new FileStream(String.Format("mandelbrot-{0}-network.txt", currentOutputIndex), FileMode.Create));

            writerNetwork.Write(finalView.Genome);
            writerNetwork.Close();

            StreamWriter writerGenerations = new StreamWriter(new FileStream(String.Format("mandelbrot-{0}-generations.txt", currentOutputIndex), FileMode.Create));

            writerGenerations.Write(fractalSelectionInstance.NumberOfGenerations);
            writerGenerations.Close();

            fractalSelectionInstance.Focus();

            currentOutputIndex++;
        }
    }
}
