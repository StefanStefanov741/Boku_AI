namespace Boku_AI
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
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

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1176, 785);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Boku AI";
            this.Text = "Boku AI";
            this.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            InitSidePanel();
            this.ResumeLayout(false);

        }

        public void InitSidePanel() {
            this.undoBtn = new System.Windows.Forms.Button();
            this.undoBtn.Location = new System.Drawing.Point(969, 64);
            this.undoBtn.Name = "undoBtn";
            this.undoBtn.Size = new System.Drawing.Size(75, 23);
            this.undoBtn.TabIndex = 0;
            this.undoBtn.Text = "Undo";
            this.undoBtn.UseVisualStyleBackColor = true;
            this.undoBtn.Click += new System.EventHandler(this.undo);
            this.Controls.Add(this.undoBtn);
        }

        #endregion

        private Button undoBtn;
    }
}