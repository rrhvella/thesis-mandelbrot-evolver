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
            this.genomeList = new System.Windows.Forms.ListBox();
            this.fractalSelectionInstance = new NEATSpacesLibrary.NEATSpaces.FractalSelection();
            this.SuspendLayout();
            // 
            // genomeList
            // 
            this.genomeList.FormattingEnabled = true;
            this.genomeList.Location = new System.Drawing.Point(-2, 322);
            this.genomeList.Name = "genomeList";
            this.genomeList.Size = new System.Drawing.Size(616, 160);
            this.genomeList.TabIndex = 1;
            // 
            // fractalSelectionInstance
            // 
            this.fractalSelectionInstance.Location = new System.Drawing.Point(-2, 0);
            this.fractalSelectionInstance.Name = "fractalSelectionInstance";
            this.fractalSelectionInstance.Size = new System.Drawing.Size(616, 326);
            this.fractalSelectionInstance.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(613, 480);
            this.Controls.Add(this.genomeList);
            this.Controls.Add(this.fractalSelectionInstance);
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "Main";
            this.ResumeLayout(false);

        }

        #endregion

        private NEATSpacesLibrary.NEATSpaces.FractalSelection fractalSelectionInstance;
        private System.Windows.Forms.ListBox genomeList;
    }
}