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
using System.Runtime.InteropServices;

namespace ComplexCPPNNEATSelection
{
    /// <summary>
    /// Controls the UI for the application.
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// The directory where the output of the application is placed.
        /// </summary>
        private const string OUTPUT_DIRECTORY = "output";

        /// <summary>
        /// Message which will be shown when an io error is raised because we can't write a file.
        /// </summary>
        private const string MESSAGE_ON_WRITE_ERROR = "Please run this executable from a directory you can write to.";
        
        /// <summary>
        /// The index of the last output collection.
        /// </summary>
        private int currentOutputIndex;

        /// <summary>
        /// If this flag is set, the application exits after initialisation.
        /// </summary>
        private bool exit;

        public MainForm()
        {
            InitializeComponent();

            //Create the output directory if it doesn't exist.
            if (!Directory.Exists(OUTPUT_DIRECTORY))
            {
                Directory.CreateDirectory(OUTPUT_DIRECTORY);
            }

            //Register the view events.
            foreach (FractalView view in fractalSelectionInstance.Views)
            {
                view.MouseEnter += new EventHandler(view_MouseEnter);
                view.Selected += new EventHandler<EventArgs>(view_Selected);
            }

            //Find the last output collection and use it to determine the index of the new one.
            var imageIndex = Directory.GetFiles(OUTPUT_DIRECTORY, "mandelbrot-*-image.png")
                                            .Select(filename => 
                                                        Int32.Parse(Regex.Match(filename, ".*mandelbrot-([0-9]*)-image.*")
                                                                         .Groups[1].Captures[0].Value)
                                                                         )
                                            .ToList();

            currentOutputIndex = (imageIndex.Count == 0)? 0 : imageIndex.Max() + 1;
        }

        /// <summary>
        /// Handles the event fired when a view is selected.
        /// </summary>
        void view_Selected(object sender, EventArgs e)
        {
            //Move the genome from the selected view to the larger one.
            finalView.Genome = (sender as FractalView).Genome;
            finalView.Refresh();
        }

        /// <summary>
        /// Handles the event fired when the mouse enters a view.
        /// </summary>
        void view_MouseEnter(object sender, EventArgs e)
        {
            //Print the textual representation of the view's genome to the 'genomeView' textbox.
            genomeView.Text = (sender as FractalView).Genome.ToString();
        }

        public static void Main(string[] args)
        {
            MainForm form;

            try
            {
                form = new MainForm();
            }
            catch (UnauthorizedAccessException e)
            {
                MessageBox.Show(MESSAGE_ON_WRITE_ERROR);
                return;
            }

            Application.Run(form); 
        }

        /// <summary>
        /// Handles the event fired when the user presses a key.
        /// </summary>
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            //When the user presses 'enter', progress to the next generation and print the number of generations to 
            //the relevant label.
            if (e.KeyCode == Keys.Enter)
            {
                fractalSelectionInstance.NextGeneration();
                generations.Text = fractalSelectionInstance.NumberOfGenerations.ToString();
            }
        }

        /// <summary>
        /// Handles the event fired when the user clicks on the 'output' button.
        /// </summary>
        private void Output_Click(object sender, EventArgs e)
        {
            fractalSelectionInstance.Focus();

            if (finalView.Genome == null)
            {
                return;
            }

            try
            {
                //Save the image in the larger view.
                finalView.FractalImage.Save(String.Format("{0}/mandelbrot-{1}-image.png", OUTPUT_DIRECTORY,
                                                            currentOutputIndex), ImageFormat.Png);

                //Save the textual representation of the genome in the larger view.
                StreamWriter writerNetwork = new StreamWriter(new FileStream(
                                                                    String.Format("{0}/mandelbrot-{1}-network.txt",
                                                                                OUTPUT_DIRECTORY,
                                                                                currentOutputIndex),
                                                                     FileMode.Create));

                writerNetwork.Write(finalView.Genome);
                writerNetwork.Close();

                //Save the number of generations to a text file.
                StreamWriter writerGenerations = new StreamWriter(new FileStream(
                                                                    String.Format("{0}/mandelbrot-{1}-generations.txt",
                                                                                 OUTPUT_DIRECTORY,
                                                                                 currentOutputIndex),
                                                                    FileMode.Create));

                writerGenerations.Write(fractalSelectionInstance.NumberOfGenerations);
                writerGenerations.Close();
            }
            catch (ExternalException ee)
            {
                MessageBox.Show(MESSAGE_ON_WRITE_ERROR);
            }
            catch (UnauthorizedAccessException uae)
            {
                MessageBox.Show(MESSAGE_ON_WRITE_ERROR);
            }

            currentOutputIndex++;
        }
    }
}
