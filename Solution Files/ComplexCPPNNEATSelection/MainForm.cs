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
        private const string OUTPUT_DIRECTORY = "output";
        private int currentOutputIndex;

        public MainForm()
        {
            InitializeComponent();

            if (!Directory.Exists(OUTPUT_DIRECTORY))
            {
                Directory.CreateDirectory(OUTPUT_DIRECTORY);
            }

            foreach (FractalView view in fractalSelectionInstance.Views)
            {
                view.MouseEnter += new EventHandler(view_MouseEnter);
                view.Selected += new EventHandler<EventArgs>(view_Selected);
            }

            var imageIndex = Directory.GetFiles(OUTPUT_DIRECTORY, "mandelbrot-*-image.png")
                                        .Select(filename => Int32.Parse(Regex.Match(filename, ".*mandelbrot-([0-9]*)-image.*").Groups[1].Captures[0].Value))
                                        .ToList();

            currentOutputIndex = (imageIndex.Count == 0)? 0 : imageIndex.Max() + 1;
        }

        void view_Selected(object sender, EventArgs e)
        {
            finalView.Genome = (sender as FractalView).Genome;
            finalView.Refresh();
        }

        void view_MouseEnter(object sender, EventArgs e)
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
            finalView.FractalImage.Save(String.Format("{0}/mandelbrot-{1}-image.png", OUTPUT_DIRECTORY, currentOutputIndex), ImageFormat.Png);

            StreamWriter writerNetwork = new StreamWriter(new FileStream(String.Format("{0}/mandelbrot-{1}-network.txt", OUTPUT_DIRECTORY, currentOutputIndex), FileMode.Create));

            writerNetwork.Write(finalView.Genome);
            writerNetwork.Close();

            StreamWriter writerGenerations = new StreamWriter(new FileStream(String.Format("{0}/mandelbrot-{1}-generations.txt", OUTPUT_DIRECTORY, currentOutputIndex), FileMode.Create));

            writerGenerations.Write(fractalSelectionInstance.NumberOfGenerations);
            writerGenerations.Close();

            fractalSelectionInstance.Focus();

            currentOutputIndex++;
        }
    }
}
