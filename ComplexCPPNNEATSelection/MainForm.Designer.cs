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

﻿namespace ComplexCPPNNEATSelection
{
    partial class MainForm
    {
                                private System.ComponentModel.IContainer components = null;

                                        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

                                        private void InitializeComponent()
        {
            this.genomeView = new System.Windows.Forms.TextBox();
            this.finalView = new ComplexCPPNNEATSelection.FractalView();
            this.fractalSelectionInstance = new ComplexCPPNNEATSelection.FractalSelection();
            this.generationalLabel = new System.Windows.Forms.Label();
            this.generations = new System.Windows.Forms.Label();
            this.Output = new System.Windows.Forms.Button();
            this.SuspendLayout();
                                                this.genomeView.Location = new System.Drawing.Point(434, 0);
            this.genomeView.Multiline = true;
            this.genomeView.Name = "genomeView";
            this.genomeView.ReadOnly = true;
            this.genomeView.Size = new System.Drawing.Size(235, 399);
            this.genomeView.TabIndex = 1;
                                                this.finalView.Genome = null;
            this.finalView.Location = new System.Drawing.Point(663, 0);
            this.finalView.Name = "finalView";
            this.finalView.Score = 0;
            this.finalView.Size = new System.Drawing.Size(400, 400);
            this.finalView.TabIndex = 6;
            this.finalView.ViewResolutionHeight = 400;
            this.finalView.ViewResolutionWidth = 400;
                                                this.fractalSelectionInstance.Location = new System.Drawing.Point(-2, 0);
            this.fractalSelectionInstance.Name = "fractalSelectionInstance";
            this.fractalSelectionInstance.Size = new System.Drawing.Size(430, 399);
            this.fractalSelectionInstance.TabIndex = 0;
                                                this.generationalLabel.AutoSize = true;
            this.generationalLabel.Location = new System.Drawing.Point(660, 412);
            this.generationalLabel.Name = "generationalLabel";
            this.generationalLabel.Size = new System.Drawing.Size(117, 13);
            this.generationalLabel.TabIndex = 7;
            this.generationalLabel.Text = "Number of generations:";
                                                this.generations.AutoSize = true;
            this.generations.Location = new System.Drawing.Point(800, 412);
            this.generations.Name = "generations";
            this.generations.Size = new System.Drawing.Size(13, 13);
            this.generations.TabIndex = 8;
            this.generations.Text = "0";
                                                this.Output.Location = new System.Drawing.Point(878, 407);
            this.Output.Name = "Output";
            this.Output.Size = new System.Drawing.Size(185, 23);
            this.Output.TabIndex = 9;
            this.Output.Text = "Output";
            this.Output.UseVisualStyleBackColor = true;
            this.Output.Click += new System.EventHandler(this.Output_Click);
                                                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1065, 483);
            this.Controls.Add(this.Output);
            this.Controls.Add(this.generations);
            this.Controls.Add(this.generationalLabel);
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
        private System.Windows.Forms.Label generationalLabel;
        private System.Windows.Forms.Label generations;
        private System.Windows.Forms.Button Output;
    }
}