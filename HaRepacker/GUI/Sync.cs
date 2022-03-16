using HaRepacker.GUI.Panels;
using MapleLib.WzLib;
using MapleLib.WzLib.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace HaRepacker.GUI
{
    public partial class Sync : Form
    {
        private MainPanel MainPanel;
        private List<WzFile> LowWz = new List<WzFile>();
        private List<WzFile> HighWz = new List<WzFile>();
        
        public Sync(MainPanel panel)
        {
            this.MainPanel = panel;
            InitializeComponent();
        }

        private async void openWz(List<WzFile> list, FolderBrowserDialog dialog, WzMapleVersion version)
        {
            Dispatcher currentDispatcher = Dispatcher.CurrentDispatcher;
            List<string> wzfilePathsToLoad = new List<string>();

            foreach (String filePath in Directory.GetFiles(dialog.SelectedPath, "*.wz", SearchOption.AllDirectories))
            {
                string filePathLowerCase = filePath.ToLower();

                if (filePathLowerCase.EndsWith("zlz.dll")) // ZLZ.dll encryption keys
                {
                    AssemblyName executingAssemblyName = Assembly.GetExecutingAssembly().GetName();
                    //similarly to find process architecture  
                    var assemblyArchitecture = executingAssemblyName.ProcessorArchitecture;

                    if (assemblyArchitecture == ProcessorArchitecture.X86)
                    {
                        ZLZPacketEncryptionKeyForm form = new ZLZPacketEncryptionKeyForm();
                        bool opened = form.OpenZLZDllFile();

                        if (opened)
                            form.Show();
                    }
                    else
                    {
                        MessageBox.Show(HaRepacker.Properties.Resources.ExecutingAssemblyError, HaRepacker.Properties.Resources.Warning, MessageBoxButtons.OK);
                    }
                    return;
                }

                // Other WZs
                else if (filePathLowerCase.EndsWith("data.wz") && WzTool.IsDataWzHotfixFile(filePath))
                {
                    WzImage img = Program.WzFileManager.LoadDataWzHotfixFile(filePath, version, MainPanel);
                    if (img == null)
                    {
                        MessageBox.Show(HaRepacker.Properties.Resources.MainFileOpenFail, HaRepacker.Properties.Resources.Error);
                        break;
                    }
                }
                else if (WzTool.IsListFile(filePath))
                {
                    //new ListEditor(filePath, WzMapleVersion.EMS).Show();
                }
                else
                {

                    wzfilePathsToLoad.Add(filePath); // add to list, so we can load it concurrently

                    // Check if there are any related files
                    string[] wzsWithRelatedFiles = { "Map", "Mob", "Skill", "Sound" };
                    bool bWithRelated = false;
                    string relatedFileName = null;

                    foreach (string wz in wzsWithRelatedFiles)
                    {
                        if (filePathLowerCase.EndsWith(wz.ToLower() + ".wz"))
                        {
                            bWithRelated = true;
                            relatedFileName = wz;
                            break;
                        }
                    }
                    if (bWithRelated)
                    {
                        if (Program.ConfigurationManager.UserSettings.AutoloadRelatedWzFiles)
                        {
                            string[] otherMapWzFiles = Directory.GetFiles(filePath.Substring(0, filePath.LastIndexOf("\\")), relatedFileName + "*.wz");
                            foreach (string filePath_Others in otherMapWzFiles)
                            {
                                if (filePath_Others != filePath)
                                    wzfilePathsToLoad.Add(filePath_Others);
                            }
                        }
                    }

                }
            }

            MainPanel.OnSetPanelLoading();
            await Task.Run(() =>
            {
                List<String> toAdd = wzfilePathsToLoad;
                ParallelLoopResult loop = Parallel.ForEach(toAdd, filePath =>
                {
                    WzFile f = Program.WzFileManager.LoadWzFile(filePath, version);
                    if (f == null)
                    {
                        // error should be thrown 
                    }
                    else
                    {
                        lock (list)
                        {
                            list.Add(f);
                        }
                    }
                });
                while (!loop.IsCompleted)
                {
                    Thread.Sleep(100); //?
                }

            });
            
        }

        private void loadwz_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            dialog.Description = "請打開低版本主程式";

            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            openWz(LowWz, dialog, WzMapleVersion.EMS);
        }

        private void syncwz_Click(object sender, EventArgs e)
        {
            MessageBox.Show(LowWz.Count.ToString());
        }
    }
}
