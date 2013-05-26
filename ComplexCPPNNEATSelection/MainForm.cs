/*
Copyright (c) 2013, robert.r.h.vella@gmail.com
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies, 
either expressed or implied, of the FreeBSD Project.
*/

﻿using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ComplexCPPNNEATSelection
{
    public partial class MainForm : Form
    {
        private const string OUTPUT_DIRECTORY = "output";

        private const string MESSAGE_ON_WRITE_ERROR = "Please run this executable from a directory you can write to.";

        private int currentOutputIndex;

        private bool exit;

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
                                .Select(filename =>
                                            Int32.Parse(Regex.Match(filename, ".*mandelbrot-([0-9]*)-image.*")
                                                             .Groups[1].Captures[0].Value)
                                                             )
                                .ToList();

            currentOutputIndex = (imageIndex.Count == 0) ? 0 : imageIndex.Max() + 1;
        }

        private void view_Selected(object sender, EventArgs e)
        {
            finalView.Genome = (sender as FractalView).Genome;
            finalView.Refresh();
        }

        private void view_MouseEnter(object sender, EventArgs e)
        {
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
            catch (IOException ie)
            {
                MessageBox.Show(MESSAGE_ON_WRITE_ERROR);
                return;
            }

            Application.Run(form);
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
            fractalSelectionInstance.Focus();

            if (finalView.Genome == null)
            {
                return;
            }

            try
            {
                finalView.FractalImage.Save(String.Format("{0}/mandelbrot-{1}-image.png", OUTPUT_DIRECTORY,
                                            currentOutputIndex), ImageFormat.Png);

                StreamWriter writerNetwork = new StreamWriter(new FileStream(
                                                    String.Format("{0}/mandelbrot-{1}-network.txt",
                                                                OUTPUT_DIRECTORY,
                                                                currentOutputIndex),
                                                     FileMode.Create));

                writerNetwork.Write(finalView.Genome);
                writerNetwork.Close();

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
            catch (IOException ie)
            {
                MessageBox.Show(MESSAGE_ON_WRITE_ERROR);
            }

            currentOutputIndex++;
        }
    }
}