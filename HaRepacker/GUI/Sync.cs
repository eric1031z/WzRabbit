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
    public partial class Sync : Form
    {
        private MainPanel MainPanel;
        private List<WzFile> LList = new List<WzFile>();
        private List<WzFile> HList = new List<WzFile>();
        private HashSet<String> ft = new HashSet<String>();
        private List<object[]> coustomWZ = new List<object[]>();
        private List<Tuple<String,StringBuilder>> js = new List<Tuple<String, StringBuilder>>();

        public Sync(MainPanel panel)
        {
            this.MainPanel = panel;
            ft.Add("navel");
            ft.Add("tamingMob");
            ft.Add("accountShareable");
            InitializeComponent();
            LinkTool.Checked = true;
            NodeFilter.Checked = true;
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
                        lock (LList)
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
            MainPanel.OnSetPanelLoadingCompleted();
            if(list.Count > 0 && list[0].Version < 200)
            {
                for(int i = 0; i < list.Count; i++)
                {
                    LLL.Text += list[i].Name.Split(new char[] { '.' })[0] + (i == list.Count - 1 ? "" : ",");
                    if (i == 8)
                    {
                        LLL.Text += "\n";
                    }
                }
            }
            else if(list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    HHH.Text += list[i].Name.Split(new char[] { '.' })[0] + (i == list.Count - 1 ? "" : ",");
                    if(i == 8)
                    {
                        HHH.Text += "\n";
                    }
                }
            }
        }

        private void 文件1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            dialog.Description = "請打開低版本主程式";
           
            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            LList = new List<WzFile>();
            LLL.Text = "";
            openWz(LList, dialog, WzMapleVersion.EMS);
            文件1位置.Text = dialog.SelectedPath;
        }

        private void 文件2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            dialog.Description = "請打開高版本主程式";

            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            HList = new List<WzFile>();
            HHH.Text = "";
            openWz(HList, dialog, WzMapleVersion.BMS);
            文件2位置.Text = dialog.SelectedPath;
        }

        private void SelectAll_Click(object sender, EventArgs e)
        {
            WZAccess.Checked = true;
            WZcap.Checked = true;
            WZCape.Checked = true;
            WZCoat.Checked = true;
            WZFace.Checked = true;
            WZGlove.Checked = true;
            WZHair.Checked = true;
            WZLongCoat.Checked = true;
            WZPants.Checked = true;
            WZPetEQ.Checked = true;
            WZRing.Checked = true;
            WZShield.Checked = true;
            WZShoe.Checked = true;
            WZTam.Checked = true;
            WZWeapon.Checked = true;
            WZDragon.Checked = true;
            WZAndroid.Checked = true;
            WZTotem.Checked = true;
            WZMechanic.Checked = true;
            WZAnimate.Checked = true;
            WZMega.Checked = true;
            WZCosume.Checked = true;
            WZChair.Checked = true;
            WZPet.Checked = true;
            WZPotential.Checked = true;
            WZEtc.Checked = true;
            WZPotential.Checked = true;
            SetItem.Checked = true;
            WZCash.Checked = true;
            DamageSkin.Checked = true;
            WZNick.Checked = true;
        }

        

        private WzFile getWzFile(List<WzFile> list, String name)
        {
            foreach(WzFile f in list)
            {
                if(f.Name == name)
                {
                    return f;
                }
            }
            return null;
        }

        private WzNode getTarget(WzFile f,String path)
        {
            String[] p = path.Split(new char[] { '/' });
            int point = 0;
            WzNode top = new WzNode(f.WzDirectory);
            while(point < p.Length)
            {
                if (top.Tag is WzImage)
                {
                    WzImage img = (WzImage)top.Tag;
                    img.ParseImage();
                    WzImageProperty prop = img.GetWzImageProperty(p[point]);
                    if (prop == null) return null;
                    top = new WzNode(prop);
                }
                else
                {
                    top = WzNode.GetChildNode(top, p[point]);
                    if (top == null) return null;
                }
                point++;
            }

            return top;
        }

        public WzFile FindTopNode(String name)
        {
            foreach (WzFile f in HList)
            {
                if (f.Name == name + ".wz")
                {
                    return f;
                }
            }
            return null;
        }

        public void ParseImg(WzNode node)
        {
            Queue<WzNode> Nodes = new Queue<WzNode>();
            Nodes.Enqueue(node);
            while (Nodes.Count > 0)
            {
                WzNode n = Nodes.Dequeue();
                ParseNode(n);
                if (n.Tag is WzImage)
                {
                    WzImage img = (WzImage)n.Tag;
                    foreach (WzImageProperty childs in img.WzProperties)
                    {
                        WzNode child = new WzNode(childs);
                        Nodes.Enqueue(child);
                    }
                }
                else
                {
                    foreach (WzNode child in n.Nodes)
                    {
                        Nodes.Enqueue(child);
                    }
                }
            }
        }

        public void ParseNode(WzNode Node)
        {
            if (Node.Text == "_inlink")
            {
                WzImage TopImage = ((WzImageProperty)Node.Tag).ParentImage;
                String[] Direction = ((WzStringProperty)Node.Tag).Value.ToString().Split(new char[] { '/' });
                WzImageProperty pointer = TopImage.GetWzImageProperty(Direction[0]);
                if (pointer == null)
                {

                    return;
                }
                for (int i = 1; i < Direction.Length; i++)
                {
                    pointer = pointer.GetProperty(Direction[i]);
                    if (pointer == null)
                    {

                        return;
                    }
                }
                if (((WzCanvasProperty)pointer).PngProperty.GetBitmap() != null)
                {
                    ((WzCanvasProperty)Node.Parent.Tag).PngProperty.SetImage(((WzCanvasProperty)pointer).PngProperty.GetBitmap());
                    ((WzCanvasProperty)Node.Parent.Tag).ParentImage.Changed = true;
                }

            }
            else if (Node.Text == "_outlink")
            {
                String[] DirectionO = ((WzStringProperty)Node.Tag).Value.ToString().Split(new char[] { '/' });
                List<String[]> tryAndFind = new List<String[]>();
                if (DirectionO[0].Contains("Map"))
                {
                    tryAndFind.Add((String[])DirectionO.Clone());
                    tryAndFind.Add((String[])DirectionO.Clone());
                    tryAndFind.Add((String[])DirectionO.Clone());
                    tryAndFind.Add((String[])DirectionO.Clone());
                    tryAndFind[0][0] = "Map";
                    tryAndFind[1][0] = "Map001";
                    tryAndFind[2][0] = "Map002";
                    tryAndFind[3][0] = "Map2";
                }
                else if (DirectionO[0].Contains("Mob"))
                {
                    tryAndFind.Add((String[])DirectionO.Clone());
                    tryAndFind.Add((String[])DirectionO.Clone());
                    tryAndFind.Add((String[])DirectionO.Clone());
                    tryAndFind[0][0] = "Mob";
                    tryAndFind[1][0] = "Mob2";
                    tryAndFind[2][0] = "Mob001";
                }
                else if (DirectionO[0].Contains("Skill"))
                {
                    tryAndFind.Add((String[])DirectionO.Clone());
                    tryAndFind.Add((String[])DirectionO.Clone());
                    tryAndFind.Add((String[])DirectionO.Clone());
                    tryAndFind[0][0] = "Skill";
                    tryAndFind[1][0] = "Skill001";
                    tryAndFind[2][0] = "Skill002";
                }
                else
                {
                    tryAndFind.Add(DirectionO);
                }


                foreach (String[] Direction in tryAndFind)
                {

                    WzFile TopNode = FindTopNode(Direction[0]);
                    if (TopNode == null)
                    {
                        continue;
                    }
                    int index = getImgIndex(Direction);

                    WzDirectory Dic = TopNode.WzDirectory;
                    Boolean findDic = true;
                    for (int i = 1; i < index; i++)
                    {
                        Dic = Dic.GetDirectoryByName(Direction[i]);
                        if (Dic == null)
                        {
                            findDic = false;
                            break;
                        }
                    }

                    if (!findDic)
                    {
                        continue;
                    }

                    WzImage Img = Dic.GetImageByName(Direction[index]);
                    if (Img == null)
                    {
                        continue;
                    }
                    WzImageProperty pointer = Img.GetWzImageProperty(Direction[index + 1]);
                    if (pointer == null)
                    {
                        continue;
                    }

                    Boolean findPointer = true;
                    for (int i = index + 2; i < Direction.Length; i++)
                    {
                        pointer = pointer.GetProperty(Direction[i]);
                        if (pointer == null)
                        {
                            findPointer = false;
                            break;
                        }
                    }
                    if (!findPointer)
                    {
                        continue;
                    }
                    if (((WzCanvasProperty)pointer).PngProperty.GetBitmap() != null)
                    {
                        ((WzCanvasProperty)Node.Parent.Tag).PngProperty.SetImage(((WzCanvasProperty)pointer).PngProperty.GetBitmap());
                        ((WzCanvasProperty)Node.Parent.Tag).ParentImage.Changed = true;
                        break;
                    }
                }

            }
        }

        private WzObject CloneWzObject(WzObject obj)
        {
            if (obj is WzImage)
            {
                return ((WzImage)obj).DeepClone(); 
            }
            else if (obj is WzImageProperty)
            {
                return ((WzImageProperty)obj).DeepClone();
            }

            return null;
        }

        private static void ParseOnDataTreeSelectedItem(WzNode selectedNode, bool expandDataTree = true)
        {
            WzImage wzImage = (WzImage)selectedNode.Tag;

            if (!wzImage.Parsed)
                wzImage.ParseImage();
            selectedNode.Reparse();
            if (expandDataTree)
            {
                selectedNode.Expand();
            }
        }

        public void DoPaste(WzNode a, WzNode b, List<HashSet<String>> filter, String memo, Boolean clean, int count)
        {
            try
            {
                // Reset replace option
                WzNode parent = a;
                WzObject parentObj = (WzObject)parent.Tag;

                if (parent != null && parent.Tag is WzImage && parent.Nodes.Count == 0)
                {
                    ParseOnDataTreeSelectedItem(parent); // only parse the main node.
                }

                if (b != null && b.Tag is WzImage && b.Nodes.Count == 0)
                {
                    ParseOnDataTreeSelectedItem(b); // only parse the main node.
                }

                if (parentObj is WzFile)
                    parentObj = ((WzFile)parentObj).WzDirectory;

                HashSet<String> allNode = new HashSet<String>();
                foreach(WzNode nd in parent.Nodes)
                {
                    allNode.Add(nd.Text);
                }

                int val = 1;
                foreach (WzNode n in b.Nodes)
                {
                    WzObject obj = (WzObject)n.Tag;
                    if (LinkTool.Checked)
                    {
                        ParseImg(n);
                    }
                    if (((obj is WzDirectory || obj is WzImage) && parentObj is WzDirectory) || (obj is WzImageProperty && parentObj is IPropertyContainer))
                    {
                        WzObject clone = CloneWzObject(obj);
                        if (clone == null)
                            continue;
                        WzNode node = new WzNode(clone, true);
                        if (!allNode.Contains(node.Text))
                        {
                            parent.AddNode(node, false);
                        }

                        if (!backgroundWorker1.CancellationPending)
                        {
                            backgroundWorker1.ReportProgress(val++ * 100 / count, memo + " - " + n.Text);
                        }
                        
                    }
                }

                if (NodeFilter.Checked && clean)
                {
                    CleanWZ(filter, parent, count, memo);
                }

                if (memo == "Cap")
                {
                    FindCap(parent);
                }

                if(memo == "Weapon" && AllWeapon.Checked)
                {
                    FixWeapon(parent, filter[2]);
                }

                if(memo == "TamingMob" && ChairFix.Checked)
                {
                    FixTamingMob(parent);
                    
                }

                if(memo == "TamingMob" && TAMItem.Checked)
                {
                    FixTam(parent);
                }

                if(memo == "ItemEff.img")
                {
                    CapeEffect(parent);
                }
                
                if(memo == "Pet")
                {
                    FixPetFly(parent);
                }

                if(memo == "MapHelper.img/AvatarMegaphone")
                {
                    FixAvatarMega(parent);
                }

            }
            finally
            {
                
            }
        }

        private int getImgIndex(String[] s)
        {
            int index = -1;
            foreach (String x in s)
            {
                if (x.Contains(".img"))
                {
                    index = s.ToList().IndexOf(x);
                }
            }
            return index;
        }

        private void SyncWZ(String name1, String name2, String p1, String p2, Boolean clean)
        {
            
            WzFile LF = getWzFile(LList, name1);
            WzFile HF = getWzFile(HList, name2);
            List<HashSet<String>> filter = new List<HashSet<String>>();
            if (LF == null || HF == null)
            {
                return;
            }
            WzNode LNode = getTarget(LF, p1);
            WzNode HNode = getTarget(HF, p2);
            if (LNode != null && HNode != null)
            {
                int count = HNode.Nodes.Count;
                if(HNode.Tag is WzImage)
                {
                    count = ((WzImage)HNode.Tag).WzProperties.Count;
                }
                if(clean) filter = FilterWZ(LNode, count, p1);
                DoPaste(LNode, HNode, filter, p1, clean, count);
            }   
        }


        private List<HashSet<String>> FilterWZ(WzNode L, int count, String type)
        {
            List<HashSet<String>> read = new List<HashSet<String>>();
            Queue<Tuple<WzNode, int>> que = new Queue<Tuple<WzNode, int>>();

            que.Enqueue(new Tuple<WzNode, int>(L, 0));
            while (que.Count > 0)
            {
                Tuple<WzNode, int> current = que.Dequeue();
                int layer = current.Item2;
                WzNode node = current.Item1;
                
                if (read.Count == layer)
                {
                    read.Add(new HashSet<String>());
                }
                if (!backgroundWorker1.CancellationPending)
                    backgroundWorker1.ReportProgress(0 / count, type + " - 正在蒐集節點訊息");
                int num = 0;
                if(layer == 2 && L.Text == "Weapon")
                {
                    read[layer].Add(node.Text);
                } else if(!int.TryParse(node.Text, out num) && !node.Text.Contains(".img")) 
                {
                    read[layer].Add(node.Text);
                }
                

                if ((WzObject)node.Tag is WzImage)
                {
                    WzImage img = (WzImage)node.Tag;
                    img.ParseImage();
                    foreach (WzImageProperty prop in img.WzProperties)
                    {
                        WzNode n = new WzNode(prop);
                        que.Enqueue(new Tuple<WzNode, int>(n, layer + 1));
                    }
                }
                else
                {
                    foreach (WzNode n in node.Nodes)
                    {
                        que.Enqueue(new Tuple<WzNode, int>(n, layer + 1));
                    }
                }
            }

            return read;
        }

        private void CleanWZ(List<HashSet<String>> filter, WzNode L, int count, String type)
        {
            Queue<Tuple<WzNode, int>> que = new Queue<Tuple<WzNode, int>>();

            que.Enqueue(new Tuple<WzNode, int>(L, 0));


            while (que.Count > 0)
            {
                Tuple<WzNode, int> current = que.Dequeue();
                int layer = current.Item2;
                WzNode node = current.Item1;

                int num = 0;

                if (!backgroundWorker1.CancellationPending)
                    this.backgroundWorker1.ReportProgress(count*100/count, type + " - 正在進行過濾節點程序");

                if(node.Text.Length > 5 && node.Tag is WzImage && (node.Text.Substring(0,5) == "01983" || node.Text.Substring(0, 5) == "01933"))
                {
                    continue;
                }

                if (layer >= filter.Count && !ft.Contains(node.Text))
                {
                    node.DeleteWzNode();
                    continue;
                }

                if (node.Text == "setItemID" || (!filter[layer].Contains(node.Text) && !node.Text.Contains(".img") && !int.TryParse(node.Text, out num) && !ft.Contains(node.Text)))
                {
                    node.DeleteWzNode();
                    continue;
                }

                if(node.Text == "tamingMob")
                {
                    WzImageProperty tam = (WzImageProperty)node.Tag;
                    int v = 0;
                    if (tam is WzStringProperty)
                    {
                        v = int.Parse(((WzStringProperty)tam).Value);
                        v -= 50000;
                        ((WzStringProperty)tam).Value = v.ToString();
                    }
                    else if (tam is WzIntProperty)
                    {
                        v = ((WzIntProperty)tam).Value;
                        ((WzIntProperty)tam).Value -= 50000;
                    }
                }



                if ((WzObject)node.Tag is WzImage)
                {
                    WzImage img = (WzImage)node.Tag;
                    img.ParseImage();
                    foreach (WzImageProperty prop in img.WzProperties.ToArray())
                    {
                        WzNode n = new WzNode(prop);
                        que.Enqueue(new Tuple<WzNode, int>(n, layer + 1));
                    }
                }
                else
                {
                    foreach (WzNode n in node.Nodes)
                    {
                        que.Enqueue(new Tuple<WzNode, int>(n, layer + 1));
                    }
                }
            }
        }

        private void FindCap(WzNode node) 
        {
            foreach (WzNode n in node.Nodes)
            {
                WzImage img = (WzImage)n.Tag;

                WzImageProperty stand1 = img.GetWzImageProperty("backDefault");
                if (stand1 != null)
                {
                    WzImageProperty bk = stand1.GetProperty("default");
                    if (bk != null)
                    {

                        WzImageProperty z = bk.GetProperty("z");
                        WzImageProperty map = bk.GetProperty("map");
                        if (z == null)
                        {
                            WzStringProperty s = new WzStringProperty("z", "backCapOverHair");
                            WzCanvasProperty ad = (WzCanvasProperty)bk;
                            ad.AddProperty(s);
                            
                        }
                        if(map != null && map.GetProperty("z") != null)
                        {
                            map.GetProperty("z").Remove();
                        }
                        img.Changed = true;
                    }
                }
            }
        }

        private void FixWeapon(WzNode Node, HashSet<String> data)
        {
            

            foreach (WzNode weapon in Node.Nodes)
            {
                if (weapon.Text.Substring(0,4) != "0170")
                {
                    continue;
                }

                WzImage u = (WzImage)weapon.Tag;
                foreach (WzImageProperty career in u.WzProperties.ToArray())
                {
                    String name = career.Name;
                    if (!data.Contains(name))
                    {
                        u.RemoveProperty(career); //清除
                    }
                }

                foreach(String s in data)
                {
                    int num = 0;
                    if(u.GetWzImageProperty(s) == null && int.TryParse(s, out num))
                    {
                        WzUOLProperty UOL = new WzUOLProperty(s, "30");
                        u.AddProperty(UOL);
                    }
                }

                u.Changed = true;
            }
        }

        private void FixTamingMob(WzNode Node)
        {

            foreach (WzNode dic in Node.Nodes)
            {
                if(dic.Text.Substring(0,4) != "0198")
                {
                    continue;
                }

                WzImage img = (WzImage)dic.Tag;
                int name = int.Parse(img.Name.Substring(1, 7)) - 50000;
                String newName = "0" + name.ToString() + ".img";

                img.Name = newName;

                WzSubProperty sit = (WzSubProperty)img.GetWzImageProperty("sit");
                if (sit != null)
                {
                    sit.Name = "stand1";
                }
                img.Changed = true;
            }
        }


        private void FixTam(WzNode Node)
        {
            WzImage imgx = ((WzDirectory)Node.Tag).GetImageByName("01912017.img");
            WzDirectory dicx = (WzDirectory)Node.Tag;
            WzImageProperty StringADD = null;
            WzImage Eqp = null;
            WzFile StringFile = getWzFile(LList, "String.wz");
            if (StringFile != null) { 
                Eqp = StringFile.WzDirectory.GetImageByName("Eqp.img");
                if(Eqp != null)
                {
                    WzImageProperty Eqp2 = Eqp.GetWzImageProperty("Eqp");
                    if(Eqp2 != null)
                    {
                        StringADD = Eqp2.GetProperty("Taming");
                    }
                }
            }
            if(imgx == null)
            {
                return;
            }
            WzImageProperty clone = imgx.GetWzImageProperty("1902024");
            WzImageProperty info = imgx.GetWzImageProperty("info");
            WzImage Universal = new WzImage("01912042.img");
            Universal.AddProperty(info.DeepClone());

            int val = 1;
            foreach (WzNode dic in Node.Nodes)
            {
                if (dic.Text.Substring(0, 5) != "01932")
                {
                    continue;
                }

                WzImage img = (WzImage)dic.Tag;
                WzImageProperty CASH = img.GetWzImageProperty("info");
                WzImageProperty isCash = CASH.GetProperty("cash");
                if(isCash == null)
                {
                    WzIntProperty cas = new WzIntProperty("cash", 1);
                    ((WzSubProperty)CASH).AddProperty(cas);
                }
                else
                {
                   if(isCash is WzIntProperty)
                    {
                        ((WzIntProperty)isCash).Value = 1;
                    }else if(isCash is WzStringProperty)
                    {
                        ((WzStringProperty)isCash).Value = "1";
                    }
                }

                int name = int.Parse(img.Name.Substring(1, 7)) - 30000;
                
                String newName = "0" + name.ToString() + ".img";

                WzSubProperty prop = new WzSubProperty(name.ToString());
                foreach(WzImageProperty index in img.WzProperties)
                {
                    if (index.Name != "info")
                    {
                        WzSubProperty sub = new WzSubProperty(index.Name);
                        prop.AddProperty(sub);
                    }
                }

                WzSubProperty addS = new WzSubProperty(name.ToString());
                WzStringProperty nname = new WzStringProperty("name",TNAME.Text + "" + val.ToString());
                addS.AddProperty(nname);
                if(StringADD != null && StringADD.GetProperty(name.ToString()) == null)
                {
                    ((WzSubProperty)StringADD).AddProperty(addS);
                }
                Universal.AddProperty(prop);
                img.Name = newName;
                img.Changed = true;
                Universal.Changed = true;
                Eqp.Changed = true;
                val++;
            }
            dicx.AddImage(Universal);
            
        }

        private int getMaxSetEffect()
        {
            WzImage Effect = getWzFile(LList, "Effect.wz").WzDirectory.GetImageByName("SetEff.img");
            int max = 0;
            foreach (WzSubProperty u in Effect.WzProperties.ToArray())
            {
                max = Math.Max(max, int.Parse(u.Name));
            }
            return max;
        }

        private WzSubProperty ImgAddSub(WzImage img, String name)
        {
            WzNode to = new WzNode(img);
            WzSubProperty toAdd = new WzSubProperty(name);
            WzNode add = new WzNode(toAdd);
            to.AddNode(add, false);

            return (WzSubProperty)img.GetWzImageProperty(name);

        }

        private WzSubProperty SubAddSub(WzSubProperty img, String name)
        {
            WzNode to = new WzNode(img);
            WzSubProperty toAdd = new WzSubProperty(name);
            WzNode add = new WzNode(toAdd);
            to.AddNode(add, false);

            return (WzSubProperty)img.GetProperty(name);
        }

        private void NodeMake(WzObject obj, WzNode node)
        {
            WzNode x = new WzNode(obj);
            x.AddNode(node,false);
        }

        private void CapeEffect(WzNode Node)
        {

            WzImage Effect = getWzFile(LList,"Effect.wz").WzDirectory.GetImageByName("SetEff.img");
            int max = getMaxSetEffect() + 1;

            foreach (WzNode e in Node.Nodes)
            {
                if(int.Parse(e.Text) < 1102000 || int.Parse(e.Text) > 1103999)
                {
                    continue;
                }

                WzImageProperty ef = ((WzImageProperty)e.Tag).GetProperty("effect");
                if (ef == null)
                {
                    continue;
                }

                WzSubProperty sub = ImgAddSub(Effect, max.ToString()); 
                WzSubProperty toAdd = SubAddSub(sub, "effect"); 
                WzSubProperty info = SubAddSub(sub, "info"); 
                WzSubProperty detail = SubAddSub(info, "9"); 
                WzStringProperty u = new WzStringProperty("0", e.Text);
                NodeMake(detail, new WzNode(u));

                foreach (WzImageProperty para in ef.WzProperties.ToArray())
                {
                    if (para.Name == "default")
                    {
                        foreach (WzImageProperty baseEff in para.WzProperties.ToArray())
                        {
                            WzNode m = new WzNode(baseEff);
                            NodeMake(toAdd, m);
                        }
                    }
                }
                max++;
            }
        }

        private void FixAvatarMega(WzNode Node)
        {
            foreach(WzNode n in Node.Nodes)
            {
                WzImageProperty p = (WzImageProperty)n.Tag;
                foreach(WzImageProperty effect in p.WzProperties.ToArray())
                {
                    if(effect.Name == "backEffect")
                    {
                        foreach(WzImageProperty prop in effect.WzProperties)
                        {
                            ((WzSubProperty)p).AddProperty(prop);
                        }
                    }

                    int num = 0;
                    if(!int.TryParse(effect.Name, out num))
                    {
                        effect.Remove();
                    }
                }
                p.ParentImage.Changed = true;
            }
        }

        public void FixPetFly(WzNode Node)
        {

            foreach (WzImage pets in ((WzDirectory)Node.Tag).GetChildImages().ToArray())
            {
                WzNode petNode = new WzNode(pets);
                foreach (WzImageProperty prop in pets.WzProperties.ToArray())
                {
                    if (prop is WzUOLProperty)
                    {

                        String linked = ((WzUOLProperty)prop).Value.ToString();
                        if (linked.Contains("/"))
                        {
                            linked = linked.Split(new char[] { '/' })[1];
                        }
                        pets.RemoveProperty(prop); 
                        WzSubProperty p = new WzSubProperty(prop.Name);
                        pets.AddProperty((WzImageProperty)p);

                        foreach (WzImageProperty index in pets.GetWzImageProperty(linked).WzProperties.ToArray())
                        {
                            p.AddProperty(index);
                        }
                    }
                }
                pets.Changed = true;
            }
        }

        public void AddDamageSkin()
        {
            int a = 1;// 
            int start = 2431000;

            WzFile HFile = getWzFile(HList, "Effect.wz");
            WzFile LFile = getWzFile(LList, "Effect.wz");

            WzFile ItemFile = getWzFile(LList, "Item.wz");
            WzFile StringFile = getWzFile(LList, "String.wz");

            WzImage address = null;
            WzImage saddress = StringFile.WzDirectory.GetImageByName("Consume.img");
            WzDirectory consume = ItemFile.WzDirectory.GetDirectoryByName("Consume");
            

            if(consume != null)
            {
                address = consume.GetImageByName("0243.img");
            }

            if (HFile == null || LFile == null) return;

            HashSet<String> set = new HashSet<String>();
            if(LFile.Version >= 145)
            {
                set.Add("resist");
                set.Add("shot");
            }
            else
            {
                set.Add("resist");
                set.Add("shot");
                set.Add("guard");
                set.Add("counter");
            }

            WzDirectory x = HFile.WzDirectory;
            WzImage LB = LFile.WzDirectory.GetImageByName("BasicEff.img");
            WzImage LE = LFile.WzDirectory.GetImageByName("ItemEff.img");
            if(LE != null && LE.GetWzImageProperty("damageSkin") == null)
            {
                WzSubProperty sub = new WzSubProperty("damageSkin");
                LE.AddProperty(sub);
            }
            
            WzSubProperty skinIcon = (WzSubProperty)LE.GetWzImageProperty("damageSkin");

            if (LB == null) return;

            WzImage basic = x.GetImageByName("BasicEff.img");
            WzImage dS = x.GetImageByName("DamageSkin.img");
            if (dS == null)
            {
                return;
            }
            WzNode node = new WzNode(dS);
            ParseImg(node);

            foreach (WzImageProperty skinx in dS.WzProperties.ToArray())
            {
                WzSubProperty skin = (WzSubProperty)skinx;
                WzSubProperty sub1 = new WzSubProperty("NoCri0_" + a.ToString());
                WzSubProperty sub2 = new WzSubProperty("NoCri1_" + a.ToString());
                WzSubProperty sub3 = new WzSubProperty("NoRed0_" + a.ToString());
                WzSubProperty sub4 = new WzSubProperty("NoRed1_" + a.ToString());

                foreach (WzImageProperty add1 in ((WzImageProperty)skin).GetProperty("NoCri0").WzProperties)
                {
                    int num = 0;
                    if(int.TryParse(add1.Name, out num))
                    {
                        sub1.AddProperty(add1.DeepClone());
                    }

                    if(LE != null && add1.Name == "7")
                    {
                        WzImageProperty toAdd = add1;
                        
                        while (toAdd is WzUOLProperty)
                        {
                            toAdd = (((WzImageProperty)skin).GetProperty("NoCri0")).GetProperty(((WzUOLProperty)toAdd).Value.ToString());
                        }
                        WzCanvasProperty icon = (WzCanvasProperty)toAdd;

                        Rectangle section = new Rectangle(new Point(0, 0), new Size(icon.PngProperty.Height, icon.PngProperty.Height));
                        Bitmap mx = CropImage(icon.PngProperty.GetBitmap(), section);
                        Bitmap re = ResizeBitmap(mx, 32, 32);
                        icon.PngProperty.SetImage(re);
                        ((WzImageProperty)icon).Name = a.ToString();
                        skinIcon.AddProperty(icon);//

                        if(address != null && DSItem.Checked)
                        {
                            StringBuilder sb = new StringBuilder();
                            WzSubProperty newDamageSkin = new WzSubProperty("0" + start.ToString());
                            WzSubProperty infox = new WzSubProperty("info");
                            WzCanvasProperty iconx = (WzCanvasProperty)icon.DeepClone();
                            WzCanvasProperty iconxRaw = (WzCanvasProperty)icon.DeepClone();
                            iconx.Name = "icon";
                            iconxRaw.Name = "iconRaw";
                            WzVectorProperty vec = (WzVectorProperty)iconx.GetProperty("origin");
                            if(vec != null)
                            {
                                vec.X.Value = 0;
                                vec.Y.Value = 32;
                            }
                            WzVectorProperty vec2 = (WzVectorProperty)iconxRaw.GetProperty("origin");
                            if(vec2 != null)
                            {
                                vec2.X.Value = 0;
                                vec2.Y.Value = 32;
                            }

                            infox.AddProperty(iconx);
                            infox.AddProperty(iconxRaw);
                            WzIntProperty tb = new WzIntProperty("tradeBlock", 1);
                            infox.AddProperty(tb);

                            WzSubProperty spec = new WzSubProperty("spec");
                            WzIntProperty npcx = new WzIntProperty("npc", 9010000);
                            WzStringProperty script = new WzStringProperty("script", start.ToString());
                            spec.AddProperty(npcx);
                            spec.AddProperty(script);

                            newDamageSkin.AddProperty(infox);
                            newDamageSkin.AddProperty(spec);
                            if (address.GetWzImageProperty("0" + start.ToString()) == null)
                            {
                                address.AddProperty(newDamageSkin);
                            }
                            
                            if(saddress != null)
                            {
                                WzSubProperty skinName = new WzSubProperty(start.ToString());
                                WzStringProperty nameS = new WzStringProperty("name", DNAME.Text + "" + a.ToString());
                                WzStringProperty desc = new WzStringProperty("desc", DNAME.Text + "" + a.ToString());
                                skinName.AddProperty(nameS);
                                skinName.AddProperty(desc);
                                saddress.AddProperty(skinName);
                            }
                            sb.Append(damageText.Text.Replace("*", a.ToString()));
                            js.Add(new Tuple<String,StringBuilder>(start.ToString(),sb));
                        }
                    }
                }

                foreach (WzImageProperty add2 in ((WzImageProperty)skin).GetProperty("NoCri1").WzProperties)
                {
                    int num = 0;
                    if (int.TryParse(add2.Name, out num) || add2.Name.Contains("effect"))
                    {
                        if (add2.Name.Contains("effect"))
                        {
                            add2.Name = "effect";
                        }
                        sub2.AddProperty(add2.DeepClone());
                    }
                }

                foreach (WzImageProperty add3 in ((WzImageProperty)skin).GetProperty("NoRed0").WzProperties)
                {
                    if (!set.Contains(add3.Name))
                        sub3.AddProperty(add3);
                }

                foreach (WzImageProperty add4 in ((WzImageProperty)skin).GetProperty("NoRed1").WzProperties)
                {
                    int num = 0;
                    if (int.TryParse(add4.Name, out num))
                    {
                        sub4.AddProperty(add4);
                    }

                }

                LB.AddProperty(sub1);
                LB.AddProperty(sub2);
                LB.AddProperty(sub3);
                LB.AddProperty(sub4);
                a++;
                start++;
                this.backgroundWorker1.ReportProgress(a * 100 / dS.WzProperties.Count, skinx.Name);
                Thread.Sleep(1);
            }

            WzImageProperty a1 = LB.GetWzImageProperty("NoCri0").DeepClone();
            a1.Name = "NoCri0_0";
            WzImageProperty a2 = LB.GetWzImageProperty("NoCri1").DeepClone();
            a2.Name = "NoCri1_0";
            WzImageProperty a3 = LB.GetWzImageProperty("NoRed0").DeepClone();
            a3.Name = "NoRed0_0";
            WzImageProperty a4 = LB.GetWzImageProperty("NoRed1").DeepClone();
            a4.Name = "NoRed1_0";

            LB.AddProperty(a1);
            LB.AddProperty(a2);
            LB.AddProperty(a3);
            LB.AddProperty(a4);
            LB.Changed = true;
            LE.Changed = true;
            if(address != null) address.Changed = true;
            if (saddress != null) saddress.Changed = true;
        }


        public Bitmap CropImage(Bitmap source, Rectangle section)
        {
            var bitmap = new Bitmap(section.Width, section.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
                return bitmap;
            }
        }


        public Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }
            return result;
        }




        private void start_Click(object sender, EventArgs e)
        {
            List<Object[]> task = new List<object[]>();
            foreach (ListViewItem item in Table.Items)
            {
                object[] obj = new object[] { item.SubItems[1].Text, item.SubItems[2].Text, item.SubItems[3].Text, item.SubItems[4].Text, item.SubItems[5].Text};
                task.Add(obj);
            }            

            if (WZcap.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "Cap", "Cap", "1"};
                task.Add(para);
            }

            if (WZCoat.Checked)
            {
                object[] para = new object[] {"Character.wz", "Character.wz", "Coat", "Coat", "1" };
                task.Add(para);
            }

            if (WZFace.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "Face", "Face", "1" };
                task.Add(para);
            }

            if (WZHair.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "Hair", "Hair", "1" };
                task.Add(para);
            }

            if (WZLongCoat.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "Longcoat", "Longcoat", "1" };
                task.Add(para);
            }

            if (WZShoe.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "Shoe", "Shoe", "1" };
                task.Add(para);
            }

            if (WZShield.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "Shield", "Sheild", "1" };
                task.Add(para);
            }

            if (WZWeapon.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "Weapon", "Weapon", "1" };
                task.Add(para);
            }

            if (WZTam.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "TamingMob", "TamingMob", "1" };
                task.Add(para);
            }

            if (WZCape.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "Cape", "Cape", "1" };
                object[] para2 = new object[] { "Effect.wz", "Effect.wz", "ItemEff.img", "ItemEff.img", "0" };
                task.Add(para);
                task.Add(para2);
            }

            if (WZPants.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "Pants", "Pants", "1" };
                task.Add(para);
            }

            if (WZPetEQ.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "PetEquip", "PetEquip", "1" };
                task.Add(para);
            }

            if (WZRing.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "Ring", "Ring", "1" };
                object[] para2 = new object[] { "UI.wz", "UI.wz", "ChatBalloon.img", "ChatBalloon.img", "0" };
                object[] para3 = new object[] { "UI.wz", "UI.wz", "NameTag.img", "NameTag.img", "0" };
                task.Add(para);
                task.Add(para2);
                task.Add(para3);
            }

            if (WZGlove.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "Glove", "Glove", "1" };
                task.Add(para);
            }

            if (WZAccess.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "Accessory", "Accessory", "1" };
                object[] para2 = new object[] { "UI.wz", "UI.wz", "NameTag.img/medal", "NameTag.img/medal", "0" };
                task.Add(para);
                task.Add(para2);
            }

            if (WZAnimate.Checked)
            {
                object[] para = new object[] { "Item.wz", "Item.wz", "Cash/0501.img", "Cash/0501.img", "0" };
                task.Add(para);
            }

            if (WZMega.Checked)
            {
                object[] para = new object[] { "Item.wz", "Item.wz", "Cash/0539.img", "Cash/0539.img", "0"};
                object[] para2 = new object[] { "Map.wz", "Map.wz", "MapHelper.img/AvatarMegaphone", "MapHelper.img/AvatarMegaphone", "0" };
                task.Add(para);
                task.Add(para2);
            }

            if (WZCosume.Checked)
            {
                WzFile LF = getWzFile(LList, "Item.wz");
                if (LF != null)
                {
                    WzDirectory dic = LF.WzDirectory.GetDirectoryByName("Consume");
                    if (dic != null)
                    {
                        foreach (WzImage img in dic.WzImages)
                        {
                            object[] para = new object[] { "Item.wz", "Item.wz", "Consume/" + img.Name, "Consume/" + img.Name, "0" };
                            task.Add(para);
                        }
                    }
                }

                object[] paran = new object[] { "Item.wz", "Item.wz", "Consume", "Consume", "0" };
                task.Add(paran);
            }

            if (WZCosume.Checked)
            {
                WzFile LF = getWzFile(LList, "Item.wz");
                if (LF != null)
                {
                    WzDirectory dic = LF.WzDirectory.GetDirectoryByName("Cash");
                    if (dic != null)
                    {
                        foreach (WzImage img in dic.WzImages)
                        {
                            object[] para = new object[] { "Item.wz", "Item.wz", "Cash/" + img.Name, "Cash/" + img.Name, "0" };
                            task.Add(para);
                        }
                    }
                }

                object[] paran = new object[] { "Item.wz", "Item.wz", "Consume", "Consume", "0" };
                task.Add(paran);
            }

            if (WZPet.Checked)
            {
                object[] para = new object[] { "Item.wz", "Item.wz", "Pet", "Pet", "1" };
                object[] para2 = new object[] { "UI.wz", "UI.wz", "NameTag.img/pet", "NameTag.img/pet", "0" };
                object[] para3 = new object[] { "UI.wz", "UI.wz", "ChatBalloon.img/pet", "ChatBalloon.img/pet", "0" };
                task.Add(para);
                task.Add(para2);
                task.Add(para3);
            }

            if (WZEtc.Checked)
            {
                WzFile LF = getWzFile(LList,"Item.wz");
                if(LF != null)
                {
                    WzDirectory dic = LF.WzDirectory.GetDirectoryByName("Etc");
                    if(dic != null)
                    {
                        foreach (WzImage img in dic.WzImages)
                        {
                            object[] para = new object[] { "Item.wz", "Item.wz", "Etc/" + img.Name, "Etc/" + img.Name, "0" };
                            task.Add(para);
                        }
                    }
                }

                object[] paran = new object[] { "Item.wz", "Item.wz", "Etc", "Etc", "0" };
                task.Add(paran);
            }

            if (WZDragon.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "Dragon", "Dragon", "1" };
                task.Add(para);
            }

            if (WZTotem.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "Totem", "Totem", "1" };
                task.Add(para);
            }

            if (WZMechanic.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "Mechanic", "Mechanic", "1" };
                task.Add(para);
            }

            if (WZAndroid.Checked)
            {
                object[] para = new object[] { "Character.wz", "Character.wz", "Android", "Android", "1" };
                task.Add(para);
            }

            if (DamageSkin.Checked)
            {
                AddDamageSkin();
                DamageSkin.ForeColor = Color.Red;
            }

            if (WZPotential.Checked)
            {
                object[] para = new object[] { "Item.wz", "Item.wz", "ItemOption.img", "ItemOption.img", "0" };
                task.Add(para);
            }

            if (SetItem.Checked)
            {
                object[] para = new object[] { "Etc.wz", "Etc.wz", "SetItemInfo.img", "SetItemInfo.img", "0" };
                task.Add(para);
            }

            if (WZNick.Checked)
            {
                object[] para = new object[] { "Item.wz", "Item.wz", "Install/0370.img", "Install/0370.img", "0" };
                object[] para2 = new object[] { "UI.wz", "UI.wz", "NameTag.img/nick", "NameTag.img/nick", "0" };
                task.Add(para);
                task.Add(para2);
            }

            if (BASIC.Checked)
            {
                object[] para = new object[] { "String.wz", "String.wz", "Cash.img", "Cash.img", "0" };
                object[] para2 = new object[] { "String.wz", "String.wz", "Consume.img", "Consume.img", "0" };
                object[] para3 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Accessory", "Eqp.img/Eqp/Accessory", "0" };
                object[] para4 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Android", "Eqp.img/Eqp/Android", "0" };
                object[] para5 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Cap", "Eqp.img/Eqp/Cap", "0" };
                object[] para6 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Cape", "Eqp.img/Eqp/Cape", "0" };
                object[] para7 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Coat", "Eqp.img/Eqp/Coat", "0" };
                object[] para8 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Dragon", "Eqp.img/Eqp/Dragon", "0" };
                object[] para9 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Face", "Eqp.img/Eqp/Face", "0" };
                object[] para10 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Glove", "Eqp.img/Eqp/Glove", "0" };
                object[] para11 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Hair", "Eqp.img/Eqp/Hair", "0" };
                object[] para12 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Longcoat", "Eqp.img/Eqp/Longcoat", "0" };
                object[] para13 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Mechanic", "Eqp.img/Eqp/Mechanic", "0" };
                object[] para14 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Pants", "Eqp.img/Eqp/Pants", "0" };
                object[] para15 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/PetEquip", "Eqp.img/Eqp/PetEquip", "0" };
                object[] para16 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Ring", "Eqp.img/Eqp/Ring", "0" };
                object[] para17 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Shield", "Eqp.img/Eqp/Shield", "0" };
                object[] para18 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Shoes", "Eqp.img/Eqp/Shoes", "0" };
                object[] para19 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Taming", "Eqp.img/Eqp/Taming", "0" };
                object[] para20 = new object[] { "String.wz", "String.wz", "Eqp.img/Eqp/Weapon", "Eqp.img/Eqp/Weapon", "0" };
                object[] para21 = new object[] { "String.wz", "String.wz", "Etc.img/Etc", "Etc.img/Etc", "0" };
                object[] para22 = new object[] { "String.wz", "String.wz", "Ins.img", "Ins.img", "0" };
                object[] para23 = new object[] { "String.wz", "String.wz", "Mob.img", "Mob.img", "0" };
                object[] para24 = new object[] { "String.wz", "String.wz", "Npc.img", "Npc.img", "0" };
                object[] para25 = new object[] { "String.wz", "String.wz", "Pet.img", "Pet.img", "0" };
                object[] para26 = new object[] { "String.wz", "String.wz", "PetDialog.img", "PetDialog.img", "0" };
                object[] para27 = new object[] { "String.wz", "String.wz", "ToolTipHelp.img", "ToolTipHelp.img", "0" };
                object[] para28 = new object[] { "UI.wz", "UI.wz", "UIWindow.img/MobGage/Mob", "UIWindow2.img/MobGage/Mob", "0" };
                task.Add(para);
                task.Add(para2);
                task.Add(para3);
                task.Add(para4);
                task.Add(para5);
                task.Add(para6);
                task.Add(para7);
                task.Add(para8);
                task.Add(para9);
                task.Add(para10);
                task.Add(para11);
                task.Add(para12);
                task.Add(para13);
                task.Add(para14);
                task.Add(para15);
                task.Add(para16);
                task.Add(para17);
                task.Add(para18);
                task.Add(para19);
                task.Add(para20);
                task.Add(para21);
                task.Add(para22);
                task.Add(para23);
                task.Add(para24);
                task.Add(para25);
                task.Add(para26);
                task.Add(para27);
                task.Add(para28);

            }


            if (WZChair.Checked)
            {

                object[] para = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/03010.img", "0" };
                object[] para2 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/03011.img", "0" };
                object[] para3 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/03012.img", "0" };
                object[] para4 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/03013.img", "0" };
                object[] para5 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/03014.img", "0" };
                object[] para6 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/030150.img", "0" };
                object[] para7 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/030151.img", "0" };
                object[] para8 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/030152.img", "0" };
                object[] para9 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/030153.img", "0" };
                object[] para10 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/030154.img", "0" };
                object[] para11 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/030155.img", "0" };
                object[] para12 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/030156.img", "0" };
                object[] para13 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/030157.img", "0" };
                object[] para14 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/030158.img", "0" };
                object[] para15 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/030159.img", "0" };
                object[] para16 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/03016.img", "0" };
                object[] para17 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/03017.img", "0" };
                object[] para18 = new object[] { "Item.wz", "Item.wz", "Install/0301.img", "Install/03018.img", "0" };
                task.Add(para);
                task.Add(para2);
                task.Add(para3);
                task.Add(para4);
                task.Add(para5);
                task.Add(para6);
                task.Add(para7);
                task.Add(para8);
                task.Add(para9);
                task.Add(para10);
                task.Add(para11);
                task.Add(para12);
                task.Add(para13);
                task.Add(para14);
                task.Add(para15);
                task.Add(para16);
                task.Add(para17);
                task.Add(para18);
            }

            this.backgroundWorker1.RunWorkerAsync(task);
        }

        void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Object[]> task = e.Argument as List<Object[]>;
            foreach(object[] arg in task)
            {
                SyncWZ(arg[0].ToString(), arg[1].ToString(), arg[2].ToString(), arg[3].ToString(), arg[4].ToString() == "1" ? true : false);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PBAR.Value = e.ProgressPercentage;
            String message = e.UserState.ToString();
            test.Text = string.Format(message + " 目前進度...{0}%", e.ProgressPercentage);
            PBAR.Update();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.test.Text = "同步程序已完成";
            MessageBox.Show("同步程序已完成,請點選保存按鍵後選擇儲存資料夾");

        }

        private void WZADD_Click(object sender, EventArgs e)
        {

            if (LList.Count == 0 || HList.Count == 0)
            {
                MessageBox.Show("請正確載入WZ主程式");
                return;
            }

            if (!LWZ.Text.Contains(".wz") || !HWZ.Text.Contains(".wz"))
            {
                MessageBox.Show("請填寫正確的WZ檔名, 如Item.wz");
                return;
            }

            List<Object[]> task = new List<object[]>();
            object[] para = new object[] { LWZ.Text, HWZ.Text, LWZAD.Text, HWZAD.Text, "0"};
            ListViewItem item = new ListViewItem((Table.Items.Count + 1).ToString());
            item.SubItems.Add(LWZ.Text);
            item.SubItems.Add(HWZ.Text);
            item.SubItems.Add(LWZAD.Text);
            item.SubItems.Add(HWZAD.Text);
            item.SubItems.Add(過濾.Checked ? "1" : "0");
            Table.Items.Add(item);
            LWZ.Clear();
            HWZ.Clear();
            LWZAD.Clear();
            HWZAD.Clear();
            過濾.Checked = false;
        }


        private void Remove_Click(object sender, EventArgs e)
        {
            if (Table.SelectedItems.Count == 0) return;

            Table.Items.Remove(Table.SelectedItems[0]);
        }

        private void modify_Click(object sender, EventArgs e)
        {
            if (Table.SelectedItems.Count == 0) return;

            if (LWZ.Text == "" && HWZ.Text == "" && LWZAD.Text == "" && HWZAD.Text == "")
            {
                ListViewItem item = Table.SelectedItems[0];
                LWZ.Text = item.SubItems[1].Text;
                HWZ.Text = item.SubItems[2].Text;
                LWZAD.Text = item.SubItems[3].Text;
                HWZAD.Text = item.SubItems[4].Text;
                過濾.Checked = item.SubItems[5].Text == "1" ? true : false;
            }
            else
            {
                Table.SelectedItems[0].SubItems[1].Text = LWZ.Text;
                Table.SelectedItems[0].SubItems[2].Text = HWZ.Text;
                Table.SelectedItems[0].SubItems[3].Text = LWZAD.Text;
                Table.SelectedItems[0].SubItems[4].Text = HWZAD.Text;
                Table.SelectedItems[0].SubItems[5].Text = 過濾.Checked ? "1" : "0";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (LList.Count == 0)
            {
                MessageBox.Show("您並無開啟任何低版本WZ");
                return;
            }

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "請選擇儲存的資料夾位置";

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            foreach (WzFile LF in LList)
            {
                if (!LF.Name.Contains("List") && !LF.Name.Contains("Base"))
                {
                    LF.SaveToDisk(dialog.SelectedPath + "/" + LF.Name, WzMapleVersion.EMS);
                }
            }

            if (!Directory.Exists(dialog.SelectedPath + "/damageSkin"))
            {
                Directory.CreateDirectory(dialog.SelectedPath + "/damageSkin");
            }

            foreach(Tuple<String,StringBuilder> sb in js)
            {
                File.WriteAllText(Path.Combine(dialog.SelectedPath + "/damageSkin", sb.Item1 + ".js"), sb.Item2.ToString());
            }
            


            Close();
        }


        private void CreateJS(string jsPath, string[] contents)
        {
            File.Create(jsPath);
            StreamWriter sw = new StreamWriter(jsPath);

            foreach (string s in contents)
            {
                sw.WriteLine(s);
            }
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                backgroundWorker1.CancelAsync();
            }
            MessageBox.Show("已停止同步程序");
            this.test.Text = "已停止同步程序";

        }
    }
}
