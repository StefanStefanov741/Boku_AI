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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            InitSidePanel(false,false);
            this.SuspendLayout();
            //Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.ClientSize = new System.Drawing.Size(1176, 785);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Boku AI";
            this.ResumeLayout(false);

        }

        public void InitSidePanel(bool pl1AI,bool pl2AI) {
            //Player 1 Label
            this.player1label = new System.Windows.Forms.Label();
            this.player1label.AutoSize = true;
            this.player1label.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.player1label.Location = new System.Drawing.Point(954, 58);
            this.player1label.Name = "player1label";
            this.player1label.Size = new System.Drawing.Size(90, 15);
            this.player1label.TabIndex = 0;
            this.player1label.Text = "Player 1 (White)";
            this.Controls.Add(this.player1label);
            //Player 1 AI combo box
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.Items.AddRange(new object[] { "Player", "AI" });
            if (!pl1AI) {
                this.comboBox1.SelectedIndex = 0;
            }
            else
            {
                this.comboBox1.SelectedIndex = 1;
            }
            this.comboBox1.Location = new System.Drawing.Point(954, 82);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 23);
            this.comboBox1.TabIndex = 0;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.changePlayer1);
            this.Controls.Add(this.comboBox1);
            //AI 1 time per move box
            this.AI1_timePerMove_box = new System.Windows.Forms.TextBox();
            this.AI1_timePerMove_box.Location = new System.Drawing.Point(1081, 82);
            this.AI1_timePerMove_box.Name = "textBox1";
            this.AI1_timePerMove_box.Size = new System.Drawing.Size(35, 23);
            this.AI1_timePerMove_box.TabIndex = 1;
            this.AI1_timePerMove_box.Text = "5";
            this.AI1_timePerMove_box.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AI_timePerMove_box_numberHandler);
            this.Controls.Add(this.AI1_timePerMove_box);
            //Label for seconds AI1
            this.AI1_s_label = new System.Windows.Forms.Label();
            this.AI1_s_label.Text = "s";
            this.AI1_s_label.ForeColor = System.Drawing.Color.White;
            this.AI1_s_label.Location = new System.Drawing.Point(1116, 86);
            this.AI1_s_label.AutoSize = true;
            this.Controls.Add(this.AI1_s_label);

            //Player 2 Label
            this.player2label = new System.Windows.Forms.Label();
            this.player2label.AutoSize = true;
            this.player2label.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.player2label.Location = new System.Drawing.Point(954, 130);
            this.player2label.Name = "player2label";
            this.player2label.Size = new System.Drawing.Size(90, 15);
            this.player2label.TabIndex = 0;
            this.player2label.Text = "Player 2 (Black)";
            this.Controls.Add(this.player2label);
            //Player 2 AI combo box
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.Items.AddRange(new object[] { "Player", "AI" });
            if (!pl2AI)
            {
                this.comboBox2.SelectedIndex = 0;
            }
            else
            {
                this.comboBox2.SelectedIndex = 1;
            }
            this.comboBox2.Location = new System.Drawing.Point(954, 158);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(121, 23);
            this.comboBox2.TabIndex = 0;
            this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.changePlayer2);
            this.Controls.Add(this.comboBox2);
            //AI 2 time per move box
            this.AI2_timePerMove_box = new System.Windows.Forms.TextBox();
            this.AI2_timePerMove_box.Location = new System.Drawing.Point(1081, 158);
            this.AI2_timePerMove_box.Name = "textBox1";
            this.AI2_timePerMove_box.Size = new System.Drawing.Size(35, 23);
            this.AI2_timePerMove_box.TabIndex = 1;
            this.AI2_timePerMove_box.Text = "5";
            this.AI2_timePerMove_box.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AI_timePerMove_box_numberHandler);
            this.Controls.Add(this.AI2_timePerMove_box);
            //Label for seconds AI1
            this.AI2_s_label = new System.Windows.Forms.Label();
            this.AI2_s_label.Text = "s";
            this.AI2_s_label.ForeColor = System.Drawing.Color.White;
            this.AI2_s_label.Location = new System.Drawing.Point(1116, 162);
            this.AI2_s_label.AutoSize = true;
            this.Controls.Add(this.AI2_s_label);

            //Undo Button
            this.undoBtn = new System.Windows.Forms.Button();
            this.undoBtn.Location = new System.Drawing.Point(954, 230);
            this.undoBtn.Name = "undoBtn";
            this.undoBtn.Size = new System.Drawing.Size(75, 23);
            this.undoBtn.TabIndex = 0;
            this.undoBtn.Text = "Undo";
            this.undoBtn.UseVisualStyleBackColor = true;
            this.undoBtn.Click += new System.EventHandler(this.undo);   
            this.Controls.Add(this.undoBtn);

            //Restart Button
            this.restartBtn = new System.Windows.Forms.Button();
            this.restartBtn.Location = new System.Drawing.Point(954, 280);
            this.restartBtn.Name = "restartBtn";
            this.restartBtn.Size = new System.Drawing.Size(75, 23);
            this.restartBtn.TabIndex = 0;
            this.restartBtn.Text = "Restart";
            this.restartBtn.UseVisualStyleBackColor = true;
            this.restartBtn.Click += new System.EventHandler(this.restartGame);
            this.Controls.Add(this.restartBtn);
        }

        private void AI_timePerMove_box_numberHandler(object sender, KeyPressEventArgs e)
        {
            //Allow only digits, backspace and delete
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b' && e.KeyChar != 127)
            {
                e.Handled = true;
            }
        }


        #endregion

        private Button undoBtn;
        private Button restartBtn;
        private Label player1label;
        private Label player2label;
        private ComboBox comboBox1;
        private ComboBox comboBox2;
        private TextBox AI1_timePerMove_box;
        private TextBox AI2_timePerMove_box;
        private Label AI1_s_label;
        private Label AI2_s_label;
    }
}