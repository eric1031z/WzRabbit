namespace HaRepacker.GUI
{
    partial class Sync
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
            this.loadwz = new System.Windows.Forms.Button();
            this.syncwz = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // loadwz
            // 
            this.loadwz.Location = new System.Drawing.Point(27, 32);
            this.loadwz.Name = "loadwz";
            this.loadwz.Size = new System.Drawing.Size(147, 50);
            this.loadwz.TabIndex = 0;
            this.loadwz.Text = "載入欲同步主程式";
            this.loadwz.UseVisualStyleBackColor = true;
            this.loadwz.Click += new System.EventHandler(this.loadwz_Click);
            // 
            // syncwz
            // 
            this.syncwz.Location = new System.Drawing.Point(27, 104);
            this.syncwz.Name = "syncwz";
            this.syncwz.Size = new System.Drawing.Size(146, 48);
            this.syncwz.TabIndex = 1;
            this.syncwz.Text = "載入高版本主程式";
            this.syncwz.UseVisualStyleBackColor = true;
            this.syncwz.Click += new System.EventHandler(this.syncwz_Click);
            // 
            // Sync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.syncwz);
            this.Controls.Add(this.loadwz);
            this.Name = "Sync";
            this.Text = "Sync";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button loadwz;
        private System.Windows.Forms.Button syncwz;
    }
}