using HaRepacker.GUI.Panels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HaRepacker.GUI
{
    public partial class MPanel : Form
    {
        private MainPanel MP;
        private Sync SyncPanel;
        private MapSync MSPanel;

        public MPanel(MainPanel mp)
        {
            this.MP = mp;
            InitializeComponent();
        }

        private void MainPage_Click(object sender, EventArgs e)
        {
            this.SwitchPanel("main");
        }

        internal void SwitchPanel(String panel)
        {
            this.pMain.Controls.Clear();
            switch (panel)
            {
                case "main":
                    this.SyncPanel = new Sync(MP);
                    this.SyncPanel.TopLevel = false;
                    this.SyncPanel.Visible = true;
                    this.pMain.Controls.Add(SyncPanel);
                    this.SyncPanel.Dock = System.Windows.Forms.DockStyle.Fill;
                    this.SyncPanel.Location = new System.Drawing.Point(0, 0);
                    this.SyncPanel.Name = "SyncPanel";
                    this.SyncPanel.Size = new System.Drawing.Size(1309, 469);
                    this.Size = new System.Drawing.Size(1480, 520);
                    break;
                case "map":
                    this.MSPanel = new MapSync(MP);
                    this.MSPanel.TopLevel = false;
                    this.MSPanel.Visible = true;
                    this.pMain.Controls.Add(MSPanel);
                    this.MSPanel.Dock = System.Windows.Forms.DockStyle.Fill;
                    this.MSPanel.Location = new System.Drawing.Point(0, 0);
                    this.MSPanel.Name = "SyncPanel";
                    this.MSPanel.Size = new System.Drawing.Size(667, 328);
                    this.Size = new System.Drawing.Size(817, 400);
                    break;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.SwitchPanel("map");
        }
    }
}
