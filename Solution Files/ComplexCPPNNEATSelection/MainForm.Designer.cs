﻿namespace ComplexCPPNNEATSelection
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.genomeView = new System.Windows.Forms.TextBox();
            this.finalView = new ComplexCPPNNEATSelection.FractalView();
            this.fractalSelectionInstance = new ComplexCPPNNEATSelection.FractalSelection();
            this.SuspendLayout();
            // 
            // genomeView
            // 
            this.genomeView.Location = new System.Drawing.Point(434, 0);
            this.genomeView.Multiline = true;
            this.genomeView.Name = "genomeView";
            this.genomeView.ReadOnly = true;
            this.genomeView.Size = new System.Drawing.Size(235, 399);
            this.genomeView.TabIndex = 1;
            // 
            // finalView
            // 
            this.finalView.Escape = 100;
            this.finalView.Genome = null;
            this.finalView.Location = new System.Drawing.Point(769, 75);
            this.finalView.Name = "finalView";
            this.finalView.Score = 0;
            this.finalView.Size = new System.Drawing.Size(200, 200);
            this.finalView.TabIndex = 6;
            this.finalView.ViewHeight = 200;
            this.finalView.ViewWidth = 200;
            // 
            // fractalSelectionInstance
            // 
            this.fractalSelectionInstance.Location = new System.Drawing.Point(-2, 0);
            this.fractalSelectionInstance.Name = "fractalSelectionInstance";
            this.fractalSelectionInstance.Size = new System.Drawing.Size(430, 399);
            this.fractalSelectionInstance.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1065, 396);
            this.Controls.Add(this.finalView);
            this.Controls.Add(this.genomeView);
            this.Controls.Add(this.fractalSelectionInstance);
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "Main";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private FractalSelection fractalSelectionInstance;
        private System.Windows.Forms.TextBox genomeView;
        private FractalView finalView;
    }
}