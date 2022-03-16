namespace HaRepacker.GUI
{
    partial class MapSync
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
            this.LOWMAP = new System.Windows.Forms.Button();
            this.HIGHMAP = new System.Windows.Forms.Button();
            this.LOWPATH = new System.Windows.Forms.Label();
            this.HIGHPATH = new System.Windows.Forms.Label();
            this.STARTMAP = new System.Windows.Forms.TextBox();
            this.ENDMAP = new System.Windows.Forms.TextBox();
            this.ADD = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.LLLA = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.PBAR = new System.Windows.Forms.ProgressBar();
            this.MAPADD = new System.Windows.Forms.TextBox();
            this.BACKADD = new System.Windows.Forms.TextBox();
            this.OBJADD = new System.Windows.Forms.TextBox();
            this.TILEADD = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.BWORK = new System.ComponentModel.BackgroundWorker();
            this.progress = new System.Windows.Forms.Label();
            this.SAVE = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LOWMAP
            // 
            this.LOWMAP.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.LOWMAP.Location = new System.Drawing.Point(43, 44);
            this.LOWMAP.Name = "LOWMAP";
            this.LOWMAP.Size = new System.Drawing.Size(93, 45);
            this.LOWMAP.TabIndex = 0;
            this.LOWMAP.Text = "低版本";
            this.LOWMAP.UseVisualStyleBackColor = true;
            this.LOWMAP.Click += new System.EventHandler(this.LOWMAP_Click);
            // 
            // HIGHMAP
            // 
            this.HIGHMAP.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.HIGHMAP.Location = new System.Drawing.Point(43, 114);
            this.HIGHMAP.Name = "HIGHMAP";
            this.HIGHMAP.Size = new System.Drawing.Size(93, 45);
            this.HIGHMAP.TabIndex = 1;
            this.HIGHMAP.Text = "高版本";
            this.HIGHMAP.UseVisualStyleBackColor = true;
            this.HIGHMAP.Click += new System.EventHandler(this.HIGHMAP_Click);
            // 
            // LOWPATH
            // 
            this.LOWPATH.AutoSize = true;
            this.LOWPATH.Location = new System.Drawing.Point(148, 60);
            this.LOWPATH.Name = "LOWPATH";
            this.LOWPATH.Size = new System.Drawing.Size(53, 12);
            this.LOWPATH.TabIndex = 2;
            this.LOWPATH.Text = "尚無開啟";
            // 
            // HIGHPATH
            // 
            this.HIGHPATH.AutoSize = true;
            this.HIGHPATH.Location = new System.Drawing.Point(148, 130);
            this.HIGHPATH.Name = "HIGHPATH";
            this.HIGHPATH.Size = new System.Drawing.Size(53, 12);
            this.HIGHPATH.TabIndex = 3;
            this.HIGHPATH.Text = "尚無開啟";
            // 
            // STARTMAP
            // 
            this.STARTMAP.Location = new System.Drawing.Point(211, 190);
            this.STARTMAP.Name = "STARTMAP";
            this.STARTMAP.Size = new System.Drawing.Size(105, 22);
            this.STARTMAP.TabIndex = 4;
            // 
            // ENDMAP
            // 
            this.ENDMAP.Location = new System.Drawing.Point(211, 226);
            this.ENDMAP.Name = "ENDMAP";
            this.ENDMAP.Size = new System.Drawing.Size(105, 22);
            this.ENDMAP.TabIndex = 5;
            // 
            // ADD
            // 
            this.ADD.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.ADD.Location = new System.Drawing.Point(43, 190);
            this.ADD.Name = "ADD";
            this.ADD.Size = new System.Drawing.Size(93, 58);
            this.ADD.TabIndex = 6;
            this.ADD.Text = "開始新增";
            this.ADD.UseVisualStyleBackColor = true;
            this.ADD.Click += new System.EventHandler(this.ADD_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(327, 193);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "Map位置";
            // 
            // LLLA
            // 
            this.LLLA.AutoSize = true;
            this.LLLA.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.LLLA.Location = new System.Drawing.Point(478, 229);
            this.LLLA.Name = "LLLA";
            this.LLLA.Size = new System.Drawing.Size(59, 12);
            this.LLLA.TabIndex = 8;
            this.LLLA.Text = "Back位置";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(331, 229);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 12);
            this.label3.TabIndex = 9;
            this.label3.Text = "Obj位置";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(484, 193);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "Tile位置";
            // 
            // PBAR
            // 
            this.PBAR.Location = new System.Drawing.Point(43, 282);
            this.PBAR.Name = "PBAR";
            this.PBAR.Size = new System.Drawing.Size(586, 25);
            this.PBAR.TabIndex = 11;
            // 
            // MAPADD
            // 
            this.MAPADD.Location = new System.Drawing.Point(388, 190);
            this.MAPADD.Name = "MAPADD";
            this.MAPADD.Size = new System.Drawing.Size(86, 22);
            this.MAPADD.TabIndex = 12;
            this.MAPADD.Text = "Map.wz";
            // 
            // BACKADD
            // 
            this.BACKADD.Location = new System.Drawing.Point(543, 226);
            this.BACKADD.Name = "BACKADD";
            this.BACKADD.Size = new System.Drawing.Size(86, 22);
            this.BACKADD.TabIndex = 13;
            this.BACKADD.Text = "Map.wz";
            // 
            // OBJADD
            // 
            this.OBJADD.Location = new System.Drawing.Point(388, 226);
            this.OBJADD.Name = "OBJADD";
            this.OBJADD.Size = new System.Drawing.Size(86, 22);
            this.OBJADD.TabIndex = 14;
            this.OBJADD.Text = "Map.wz";
            // 
            // TILEADD
            // 
            this.TILEADD.Location = new System.Drawing.Point(543, 190);
            this.TILEADD.Name = "TILEADD";
            this.TILEADD.Size = new System.Drawing.Size(86, 22);
            this.TILEADD.TabIndex = 15;
            this.TILEADD.Text = "Map.wz";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label5.Location = new System.Drawing.Point(148, 193);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 12);
            this.label5.TabIndex = 16;
            this.label5.Text = "開始地圖";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label6.Location = new System.Drawing.Point(148, 229);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 12);
            this.label6.TabIndex = 17;
            this.label6.Text = "結束地圖";
            // 
            // BWORK
            // 
            this.BWORK.WorkerReportsProgress = true;
            this.BWORK.WorkerSupportsCancellation = true;
            this.BWORK.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BWORK_DoWork);
            this.BWORK.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BWORK_ProgressChanged);
            this.BWORK.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BWORK_RunWorkerCompleted);
            // 
            // progress
            // 
            this.progress.AutoSize = true;
            this.progress.Location = new System.Drawing.Point(41, 267);
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(0, 12);
            this.progress.TabIndex = 18;
            // 
            // SAVE
            // 
            this.SAVE.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.SAVE.Location = new System.Drawing.Point(333, 44);
            this.SAVE.Name = "SAVE";
            this.SAVE.Size = new System.Drawing.Size(82, 45);
            this.SAVE.TabIndex = 19;
            this.SAVE.Text = "保存檔案";
            this.SAVE.UseVisualStyleBackColor = true;
            this.SAVE.Click += new System.EventHandler(this.SAVE_Click);
            // 
            // MapSync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 328);
            this.Controls.Add(this.SAVE);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.TILEADD);
            this.Controls.Add(this.OBJADD);
            this.Controls.Add(this.BACKADD);
            this.Controls.Add(this.MAPADD);
            this.Controls.Add(this.PBAR);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.LLLA);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ADD);
            this.Controls.Add(this.ENDMAP);
            this.Controls.Add(this.STARTMAP);
            this.Controls.Add(this.HIGHPATH);
            this.Controls.Add(this.LOWPATH);
            this.Controls.Add(this.HIGHMAP);
            this.Controls.Add(this.LOWMAP);
            this.Name = "MapSync";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "地圖新增程序";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LOWMAP;
        private System.Windows.Forms.Button HIGHMAP;
        private System.Windows.Forms.Label LOWPATH;
        private System.Windows.Forms.Label HIGHPATH;
        private System.Windows.Forms.TextBox STARTMAP;
        private System.Windows.Forms.TextBox ENDMAP;
        private System.Windows.Forms.Button ADD;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label LLLA;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ProgressBar PBAR;
        private System.Windows.Forms.TextBox MAPADD;
        private System.Windows.Forms.TextBox BACKADD;
        private System.Windows.Forms.TextBox OBJADD;
        private System.Windows.Forms.TextBox TILEADD;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.ComponentModel.BackgroundWorker BWORK;
        private System.Windows.Forms.Label progress;
        private System.Windows.Forms.Button SAVE;
    }
}