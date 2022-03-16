using HaRepacker.GUI.Panels;
using MapleLib.WzLib;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;
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
    public partial class Commodity : Form
    {
        private MainPanel MainPanel;
        private List<WzFile> files = new List<WzFile>();
        public Commodity(MainPanel panel)
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
                else if(filePathLowerCase.Contains("character") || filePathLowerCase.EndsWith("etc.wz") || filePathLowerCase.EndsWith("item.wz"))
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
                    Thread.Sleep(100); 
                }

            });
            MainPanel.OnSetPanelLoadingCompleted();
        }

        private void Load_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            dialog.Description = "請打開主程式";

            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            files = new List<WzFile>();
            openWz(files, dialog, WzMapleVersion.EMS);
        }

        private Boolean CheckData()
        {
            if(files.Count < 2)
            {
                MessageBox.Show("請確認WZ已讀取");
                return false;
            }
            return true;
        }


        private WzFile getTopNode(String name)
        {
            foreach (WzFile n in files)
            {
                if (n.Name == name) return n;
            }
            return null;
        }


        private int GetIntValue(WzNode node)
        {
            if (node == null) return -1;
            int ret = 0;
            if (node.Tag is WzIntProperty) ret = ((WzIntProperty)node.Tag).Value;
            else if (node.Tag is WzStringProperty)
            {
                String s = ((WzStringProperty)node.Tag).Value;
                if (int.TryParse(s, out ret))
                {
                    ret = int.Parse(s);
                }
            }
            return ret;
        }


        private Boolean IsValidCash(Dictionary<String, int> data, List<Tuple<String, int>> check)
        {
            foreach (Tuple<String, int> pair in check)
            {
                String name = pair.Item1;
                int v = pair.Item2;
                if (!data.ContainsKey("cash")) return false;
                else if (data.ContainsKey(name) && data[name] != v)
                {
                    return false;
                }
            }
            return true;
        }



        private SortedSet<int> GetCashItem(WzNode node, List<Tuple<String, int>> check)
        {
            SortedSet<int> ret = new SortedSet<int>();
            if (node != null)
            {
                int para = 1;

                if (node.Tag is WzImage)
                {
                    WzImage image = (WzImage)node.Tag;
                    foreach (WzImageProperty pImg in image.WzProperties)
                    {
                        Dictionary<String, int> data = new Dictionary<String, int>();
                        WzImageProperty prop = pImg.GetProperty("info");
                        foreach (WzImageProperty p in prop.WzProperties)
                        {
                            WzNode child = new WzNode(p);
                            data.Add(p.Name, GetIntValue(child));
                        }

                        if (IsValidCash(data, check))
                        {
                            ret.Add(int.Parse(pImg.Name.Split(new char[] { '.' })[0]));
                        }
                        this.BWORK.ReportProgress(para * 100 / image.WzProperties.Count, "已查看 - " + pImg.Name);
                        para++;
                    }
                }
                else
                {
                    foreach (WzNode n in node.Nodes)
                    {
                        Dictionary<String, int> data = new Dictionary<String, int>();
                        if (n.Tag is WzImage)
                        {
                            WzImageProperty prop = ((WzImage)n.Tag).GetWzImageProperty("info");
                            foreach (WzImageProperty p in prop.WzProperties)
                            {
                                WzNode child = new WzNode(p);
                                data.Add(p.Name, GetIntValue(child));
                            }
                        }
                        else
                        {
                            WzNode info = WzNode.GetChildNode(n, "info");

                            foreach (WzNode p in info.Nodes)
                            {
                                data.Add(p.Text, GetIntValue(p));
                            }
                        }

                        if (IsValidCash(data, check))
                        {
                            ret.Add(int.Parse(n.Text.Split(new char[] { '.' })[0]));
                        }
                        this.BWORK.ReportProgress(para * 100 / node.Nodes.Count, "已查看 - " + n.Text);
                        para++;
                    }
                }
            }
            return ret;
        }


        private WzFile ContainFileAndGet(HashSet<String> name)
        {
            foreach(WzFile file in files){
                if (name.Contains(file.Name)) return file;
            }
            return null;
        }

        private int ImageIndex(String[] path)
        {
            for(int i = 0; i < path.Length; i++)
            {
                if (path[i].Contains("img")) return i;
            }
            return int.MaxValue;
        }


        

        private WzNode GetTargetNode(String input)
        {
            String[] path = input.Split(new char[] { '/' });
            HashSet<String> maybeName = new HashSet<String> { path[0], path[0] + ".wz" };

            WzFile file = ContainFileAndGet(maybeName);
            WzObject obj = null;

            if(file == null)
            {
                //autoload
                return null;
            }
            try
            {
                WzDirectory dic = file.WzDirectory;
                int imgPoint = ImageIndex(path);
                for (int i = 1; i < imgPoint && i < path.Length; i++)
                {
                    dic = dic.GetDirectoryByName(path[i]);
                    if (dic == null) return null;
                    obj = dic;
                }

                WzImage img = dic.GetImageByName(path[imgPoint]);
                if (img == null) return null;
                obj = img;

                WzImageProperty prop = img.GetWzImageProperty(path[++imgPoint]);
                if (prop == null) return null;
                obj = prop;

                for (int i = imgPoint + 1; i < path.Length; i++)
                {
                    prop = prop.GetProperty(path[i]);
                    if (prop == null) return null;
                    obj = prop;
                }
            }catch(Exception)
            {
               
            }

            return new WzNode(obj);
        }


        private WzImage InitCommodity()
        {
            WzFile file = getTopNode("Etc.wz");
            WzImage commodity = file.WzDirectory.GetImageByName("Commodity.img");
            if (commodity != null) commodity.Remove();
            WzImage newCommodity = new WzImage("Commodity.img");
            file.WzDirectory.AddImage(newCommodity);
            return newCommodity;
        }

        private void AddNewCommodity(WzImage img, int id, int price, int itemid, int SN)
        {
            WzSubProperty sub = new WzSubProperty(id.ToString());
            WzIntProperty count = new WzIntProperty("Count", 1);
            WzIntProperty Gender = new WzIntProperty("Gender", 2);
            WzIntProperty Itemid = new WzIntProperty("Itemid", itemid);
            WzIntProperty OnSale = new WzIntProperty("Onsale", 0);
            WzIntProperty Period = new WzIntProperty("Period", 0);
            WzIntProperty Price = new WzIntProperty("Price", price);
            WzIntProperty Priority = new WzIntProperty("Priority", 9);
            WzIntProperty SNN = new WzIntProperty("SN", SN);
            sub.AddProperty(count);
            sub.AddProperty(Gender);
            sub.AddProperty(Itemid);
            sub.AddProperty(OnSale);
            sub.AddProperty(Period);
            sub.AddProperty(Price);
            sub.AddProperty(Priority);
            sub.AddProperty(SNN);
            img.AddProperty(sub);
        }

        
        Dictionary<String, List<String>> DataType = new Dictionary<String, List<String>>();
        

        public void CreateCommodity()

        {
            if (!CheckData()) return;

          

            
            List<SortedSet<int>> data = new List<SortedSet<int>>();

            List<Tuple<String, int>> check = new List<Tuple<String, int>>();
            /**add parameter*/
            check.Add(new Tuple<String, int>("cash", 1));
            check.Add(new Tuple<String, int>("incPAD", 0));
            check.Add(new Tuple<String, int>("incMAD", 0));


            DataType.Add("Character", new List<String>(){"20000000", "Cap", "Accessory", "Accessory", "LongCoat", "Coat", "Pants", "Shoes", "Gloves", "Weapon", "Ring", "", "Cape" });

            
            WzImage img = InitCommodity();
            foreach (String topic in DataType.Keys)
            {
                List<String> type = DataType[topic];
                for(int i = 1; i < type.Count; i++)
                {
                    data.Add(GetCashItem(GetTargetNode(topic + "/" + type[i]), check));
                }

                int index = 0;
                int serial = int.Parse(type[0]);
                for (int i = 1; i < data.Count; i++)
                {
                    int num = 0;
                    foreach (int item in data[i])
                    {
                        int sn = serial + i * 100000 + num;
                        AddNewCommodity(img, index, 1, item, sn);
                        num++;
                        index++;

                    }
                }
            }
            img.Changed = true;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (!CheckData()) return;

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "請選擇儲存的資料夾位置";

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            foreach (WzFile file in files)
            {
                if (file.Name == "Etc.wz")
                {
                    file.SaveToDisk(dialog.SelectedPath + "/" + file.Name, WzMapleVersion.EMS);
                }
            }
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.BWORK.RunWorkerAsync();
        }

        private void BWORK_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PBar.Value = e.ProgressPercentage;
            String message = e.UserState.ToString();
            this.progress.Text = string.Format(message + " 目前進度...{0}%", e.ProgressPercentage);
            PBar.Update();
        }

        private void BWORK_DoWork(object sender, DoWorkEventArgs e)
        {
            CreateCommodity();
        }

        private void BWORK_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.progress.Text = "已完成";
            MessageBox.Show("完成新增");
        }

        private void Table_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(SystemBrushes.Menu, e.Bounds);
            e.Graphics.DrawRectangle(SystemPens.GradientInactiveCaption,
                new Rectangle(e.Bounds.X, 0, e.Bounds.Width, e.Bounds.Height));

            string text = Table.Columns[e.ColumnIndex].Text;
            TextFormatFlags cFlag = TextFormatFlags.HorizontalCenter
                                  | TextFormatFlags.VerticalCenter;
            TextRenderer.DrawText(e.Graphics, text, Table.Font, e.Bounds, Color.Black, cFlag);
        }

        void setLastColumnTofill(ListView lv)
        {
            int sum = 0;
            int count = lv.Columns.Count;
            for (int i = 0; i < count - 1; i++) sum += lv.Columns[i].Width;
            lv.Columns[count - 1].Width = lv.ClientSize.Width - sum;
        }



        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void AddNewPath_Click(object sender, EventArgs e)
        {

        }
    }
}
