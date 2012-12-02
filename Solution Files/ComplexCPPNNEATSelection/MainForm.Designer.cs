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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.genomeView = new System.Windows.Forms.TextBox();
            this.viewX = new System.Windows.Forms.TextBox();
            this.viewY = new System.Windows.Forms.TextBox();
            this.viewS = new System.Windows.Forms.TextBox();
            this.updateView = new System.Windows.Forms.Button();
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
            // viewX
            // 
            this.viewX.Location = new System.Drawing.Point(13, 406);
            this.viewX.Name = "viewX";
            this.viewX.Size = new System.Drawing.Size(100, 20);
            this.viewX.TabIndex = 2;
            // 
            // viewY
            // 
            this.viewY.Location = new System.Drawing.Point(147, 406);
            this.viewY.Name = "viewY";
            this.viewY.Size = new System.Drawing.Size(100, 20);
            this.viewY.TabIndex = 3;
            // 
            // viewS
            // 
            this.viewS.Location = new System.Drawing.Point(276, 405);
            this.viewS.Name = "viewS";
            this.viewS.Size = new System.Drawing.Size(100, 20);
            this.viewS.TabIndex = 4;
            // 
            // updateView
            // 
            this.updateView.Location = new System.Drawing.Point(434, 405);
            this.updateView.Name = "updateView";
            this.updateView.Size = new System.Drawing.Size(235, 23);
            this.updateView.TabIndex = 5;
            this.updateView.Text = "update view";
            this.updateView.UseVisualStyleBackColor = true;
            this.updateView.Click += new System.EventHandler(this.updateView_Click);
            // 
            // finalView
            // 
            this.finalView.Escape = 500;
            this.finalView.Genome = null;
            this.finalView.Location = new System.Drawing.Point(664, 0);
            this.finalView.Name = "finalView";
            this.finalView.Score = 0;
            this.finalView.Size = new System.Drawing.Size(400, 400);
            this.finalView.TabIndex = 6;
            this.finalView.ViewHeight = 200;
            this.finalView.ViewPosition = ((System.Numerics.Complex)(resources.GetObject("finalView.ViewPosition")));
            this.finalView.ViewSize = 200D;
            this.finalView.ViewWidth = 200;
            // 
            // fractalSelectionInstance
            // 
            this.fractalSelectionInstance.Location = new System.Drawing.Point(-2, 0);
            this.fractalSelectionInstance.Name = "fractalSelectionInstance";
            this.fractalSelectionInstance.Size = new System.Drawing.Size(430, 399);
            this.fractalSelectionInstance.TabIndex = 0;
            this.fractalSelectionInstance.ViewPosition = ((System.Numerics.Complex)(resources.GetObject("fractalSelectionInstance.ViewPosition")));
            this.fractalSelectionInstance.ViewSize = 2D;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1065, 480);
            this.Controls.Add(this.finalView);
            this.Controls.Add(this.updateView);
            this.Controls.Add(this.viewS);
            this.Controls.Add(this.viewY);
            this.Controls.Add(this.viewX);
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
        private System.Windows.Forms.TextBox viewX;
        private System.Windows.Forms.TextBox viewY;
        private System.Windows.Forms.TextBox viewS;
        private System.Windows.Forms.Button updateView;
        private FractalView finalView;
    }
}