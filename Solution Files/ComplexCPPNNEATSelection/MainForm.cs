using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Numerics;

namespace ComplexCPPNNEATSelection
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            foreach (FractalView view in fractalSelectionInstance.Views)
            {
                view.MouseEnter += new EventHandler(view_MouseHover);
                view.Selected += new EventHandler<EventArgs>(view_Selected);
            }
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
    }
}
