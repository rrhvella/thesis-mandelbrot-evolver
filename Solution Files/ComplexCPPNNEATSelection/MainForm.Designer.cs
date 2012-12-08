namespace ComplexCPPNNEATSelection
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
            this.genomeView = new System.Windows.Forms.TextBox();
            this.finalView = new ComplexCPPNNEATSelection.FractalView();
            this.fractalSelectionInstance = new ComplexCPPNNEATSelection.FractalSelection();
            this.generationalLabel = new System.Windows.Forms.Label();
            this.generations = new System.Windows.Forms.Label();
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
            this.finalView.Location = new System.Drawing.Point(663, 0);
            this.finalView.Name = "finalView";
            this.finalView.Score = 0;
            this.finalView.Size = new System.Drawing.Size(400, 400);
            this.finalView.TabIndex = 6;
            this.finalView.ViewHeight = 400;
            this.finalView.ViewWidth = 400;
            // 
            // fractalSelectionInstance
            // 
            this.fractalSelectionInstance.Location = new System.Drawing.Point(-2, 0);
            this.fractalSelectionInstance.Name = "fractalSelectionInstance";
            this.fractalSelectionInstance.Size = new System.Drawing.Size(430, 399);
            this.fractalSelectionInstance.TabIndex = 0;
            // 
            // generationalLabel
            // 
            this.generationalLabel.AutoSize = true;
            this.generationalLabel.Location = new System.Drawing.Point(660, 412);
            this.generationalLabel.Name = "generationalLabel";
            this.generationalLabel.Size = new System.Drawing.Size(117, 13);
            this.generationalLabel.TabIndex = 7;
            this.generationalLabel.Text = "Number of generations:";
            // 
            // generations
            // 
            this.generations.AutoSize = true;
            this.generations.Location = new System.Drawing.Point(783, 412);
            this.generations.Name = "generations";
            this.generations.Size = new System.Drawing.Size(13, 13);
            this.generations.TabIndex = 8;
            this.generations.Text = "0";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1065, 483);
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
    }
}