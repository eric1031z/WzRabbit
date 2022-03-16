namespace HaRepacker.GUI
{
    partial class Commodity
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
            this.Load = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.PBar = new System.Windows.Forms.ProgressBar();
            this.BWORK = new System.ComponentModel.BackgroundWorker();
            this.progress = new System.Windows.Forms.Label();
            this.WzPath = new System.Windows.Forms.TextBox();
            this.SNPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.AddNewPath = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.Table = new System.Windows.Forms.ListView();
            this.WZ檔名 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.WZ路徑 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SN碼 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // Load
            // 
            this.Load.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Load.Location = new System.Drawing.Point(40, 42);
            this.Load.Name = "Load";
            this.Load.Size = new System.Drawing.Size(212, 52);
            this.Load.TabIndex = 0;
            this.Load.Text = "載入WZ";
            this.Load.UseVisualStyleBackColor = true;
            this.Load.Click += new System.EventHandler(this.Load_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button1.Location = new System.Drawing.Point(258, 42);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(163, 52);
            this.button1.TabIndex = 1;
            this.button1.Text = "開始";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Save
            // 
            this.Save.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Save.Location = new System.Drawing.Point(427, 42);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(187, 52);
            this.Save.TabIndex = 2;
            this.Save.Text = "存檔";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // PBar
            // 
            this.PBar.Location = new System.Drawing.Point(40, 422);
            this.PBar.Name = "PBar";
            this.PBar.Size = new System.Drawing.Size(574, 36);
            this.PBar.TabIndex = 3;
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
            this.progress.Location = new System.Drawing.Point(38, 407);
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(31, 12);
            this.progress.TabIndex = 4;
            this.progress.Text = "進度";
            // 
            // WzPath
            // 
            this.WzPath.Location = new System.Drawing.Point(75, 120);
            this.WzPath.Name = "WzPath";
            this.WzPath.Size = new System.Drawing.Size(191, 22);
            this.WzPath.TabIndex = 5;
            // 
            // SNPath
            // 
            this.SNPath.Location = new System.Drawing.Point(299, 120);
            this.SNPath.Name = "SNPath";
            this.SNPath.Size = new System.Drawing.Size(122, 22);
            this.SNPath.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(38, 123);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "路徑";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(272, 123);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(21, 12);
            this.label2.TabIndex = 8;
            this.label2.Text = "SN";
            // 
            // AddNewPath
            // 
            this.AddNewPath.Location = new System.Drawing.Point(427, 120);
            this.AddNewPath.Name = "AddNewPath";
            this.AddNewPath.Size = new System.Drawing.Size(51, 23);
            this.AddNewPath.TabIndex = 9;
            this.AddNewPath.Text = "新增";
            this.AddNewPath.UseVisualStyleBackColor = true;
            this.AddNewPath.Click += new System.EventHandler(this.AddNewPath_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(530, 131);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 12);
            this.label3.TabIndex = 10;
            this.label3.Text = "已新增路徑 :/";
            // 
            // Table
            // 
            this.Table.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.WZ檔名,
            this.WZ路徑,
            this.SN碼});
            this.Table.FullRowSelect = true;
            this.Table.GridLines = true;
            this.Table.HideSelection = false;
            this.Table.Location = new System.Drawing.Point(40, 160);
            this.Table.Name = "Table";
            this.Table.OwnerDraw = true;
            this.Table.Size = new System.Drawing.Size(572, 235);
            this.Table.TabIndex = 11;
            this.Table.UseCompatibleStateImageBehavior = false;
            this.Table.View = System.Windows.Forms.View.Details;
            // 
            // WZ檔名
            // 
            this.WZ檔名.Text = "WZ檔名";
            this.WZ檔名.Width = 130;
            // 
            // WZ路徑
            // 
            this.WZ路徑.Text = "WZ路徑";
            this.WZ路徑.Width = 319;
            // 
            // SN碼
            // 
            this.SN碼.Text = "SN碼";
            this.SN碼.Width = 118;
            // 
            // Commodity
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 478);
            this.Controls.Add(this.Table);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.AddNewPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.SNPath);
            this.Controls.Add(this.WzPath);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.PBar);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Load);
            this.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Name = "Commodity";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "商城";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Load;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.ProgressBar PBar;
        private System.ComponentModel.BackgroundWorker BWORK;
        private System.Windows.Forms.Label progress;
        private System.Windows.Forms.TextBox WzPath;
        private System.Windows.Forms.TextBox SNPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button AddNewPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListView Table;
        private System.Windows.Forms.ColumnHeader WZ檔名;
        private System.Windows.Forms.ColumnHeader WZ路徑;
        private System.Windows.Forms.ColumnHeader SN碼;
    }
}