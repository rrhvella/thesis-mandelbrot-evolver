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
            this.fractalSelection1 = new NEATSpacesLibrary.NEATSpaces.FractalSelection();
            this.SuspendLayout();
            // 
            // fractalSelection1
            // 
            this.fractalSelection1.Location = new System.Drawing.Point(-2, 0);
            this.fractalSelection1.Name = "fractalSelection1";
            this.fractalSelection1.Size = new System.Drawing.Size(396, 398);
            this.fractalSelection1.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(613, 480);
            this.Controls.Add(this.fractalSelection1);
            this.Name = "MainForm";
            this.Text = "Main";
            this.ResumeLayout(false);

        }

        #endregion

        private NEATSpacesLibrary.NEATSpaces.FractalSelection fractalSelection1;
    }
}