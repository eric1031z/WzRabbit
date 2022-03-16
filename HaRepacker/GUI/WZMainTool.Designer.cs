namespace HaRepacker.GUI
{
    partial class MPanel
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.MainPage = new System.Windows.Forms.Button();
            this.pMain = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.MainPage);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(150, 481);
            this.panel1.TabIndex = 0;
            // 
            // MainPage
            // 
            this.MainPage.Location = new System.Drawing.Point(11, 141);
            this.MainPage.Name = "MainPage";
            this.MainPage.Size = new System.Drawing.Size(125, 47);
            this.MainPage.TabIndex = 0;
            this.MainPage.Text = "button1";
            this.MainPage.UseVisualStyleBackColor = true;
            this.MainPage.Click += new System.EventHandler(this.MainPage_Click);
            // 
            // pMain
            // 
            this.pMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pMain.Location = new System.Drawing.Point(150, 0);
            this.pMain.Name = "pMain";
            this.pMain.Size = new System.Drawing.Size(1314, 481);
            this.pMain.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(11, 218);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(125, 47);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // MPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(1464, 481);
            this.Controls.Add(this.pMain);
            this.Controls.Add(this.panel1);
            this.Name = "MPanel";
            this.Text = "WZMainTool";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button MainPage;
        private System.Windows.Forms.Panel pMain;
        private System.Windows.Forms.Button button1;
    }
}