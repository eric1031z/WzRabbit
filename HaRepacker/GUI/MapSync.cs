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
    public partial class MapSync : Form
    {
        private MainPanel MainPanel;
        private List<WzFile> LList = new List<WzFile>();
        private List<WzFile> HList = new List<WzFile>();

        public MapSync(MainPanel panel)
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
            MainPanel.OnSetPanelLoadingCompleted();
        }



        //////
        /// <summary>
        /// 
        /// 
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// 


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

        public List<HashSet<String>> GetMapData(List<WzNode> listNode)
        {

            HashSet<String> BackData = new HashSet<String>();
            HashSet<String> ObjData = new HashSet<String>();
            HashSet<String> TileData = new HashSet<String>();

            HashSet<String> NpcData = new HashSet<String>();
            HashSet<String> MobData = new HashSet<String>();
            HashSet<String> MapData = new HashSet<String>();
            HashSet<String> StringData = new HashSet<String>();
            HashSet<String> ReactorData = new HashSet<String>();
            HashSet<String> SoundData = new HashSet<String>();

            List<HashSet<String>> ret = new List<HashSet<String>>();
            int vv = 1;
            foreach (WzNode node in listNode)
            {
                WzImage img = (WzImage)node.Tag;
                MapData.Add(img.Name.Split(new char[] { '.' })[0]);
                WzImageProperty info = img.GetWzImageProperty("info");
                if (info != null)
                {
                    WzImageProperty bgm = info.GetProperty("bgm");
                    String nsound = ((WzStringProperty)bgm).Value;
                    SoundData.Add(nsound);
                }

                foreach (WzImageProperty prop in img.WzProperties) //內容
                {
                    int num = 0;
                    if (int.TryParse(prop.Name, out num)) //處理obj tile
                    {

                        WzImageProperty obj = prop.GetProperty("obj");
                        if (obj != null)
                        {
                            foreach (WzImageProperty item in obj.WzProperties)
                            {
                                WzImageProperty s = item.GetProperty("oS");
                                if (s != null)
                                {
                                    String oS = ((WzStringProperty)s).Value;
                                    ObjData.Add(oS);
                                }
                            }
                        }

                        WzImageProperty tile = prop.GetProperty("info");
                        if (tile != null)
                        {
                            WzImageProperty tS = tile.GetProperty("tS");
                            if (tS != null)
                            {
                                String v = ((WzStringProperty)tS).Value;
                                TileData.Add(v);
                            }
                        }
                    }
                    else if (prop.Name.Equals("back"))
                    {
                        //back
                        WzImageProperty back = prop;
                        foreach (WzImageProperty item in back.WzProperties)
                        {
                            WzImageProperty s = item.GetProperty("bS");
                            if (s == null) continue;
                            String bS = ((WzStringProperty)s).Value;
                            BackData.Add(bS);
                        }
                    }
                    else if (prop.Name.Equals("life"))
                    {
                        //life
                        WzImageProperty life = prop;
                        if (life == null) continue;

                        foreach (WzImageProperty item in life.WzProperties)
                        {
                            WzImageProperty type = item.GetProperty("type");
                            if (type == null) continue;

                            String tp = ((WzStringProperty)type).Value;
                            HashSet<String> lifeData;
                            String types = "";
                            lifeData = tp.Equals("n") ? NpcData : MobData;
                            types = tp.Equals("n") ? "Npc.img" : "Mob.img";
                            WzImageProperty id = item.GetProperty("id");
                            if (id == null) continue;
                            if (id is WzIntProperty)
                            {
                                String i = ((WzIntProperty)id).Value.ToString();
                                lifeData.Add(i);
                                StringData.Add(types + "/" + i);
                                if (types.Equals("Mob.img"))
                                {
                                    SoundData.Add("Mob/" + i);
                                }
                            }
                            else if (id is WzStringProperty)
                            {
                                String i = ((WzStringProperty)id).Value;
                                lifeData.Add(i);
                                StringData.Add(types + "/" + i);
                                if (types.Equals("Mob.img"))
                                {
                                    SoundData.Add("Mob/" + i);
                                }
                            }
                        }
                    }
                    else if (prop.Name.Equals("reactor"))
                    {
                        WzImageProperty reac = prop;
                        foreach (WzImageProperty item in reac.WzProperties)
                        {
                            WzImageProperty id = item.GetProperty("id");
                            if (id is WzIntProperty)
                            {
                                String i = ((WzIntProperty)id).Value.ToString();
                                ReactorData.Add(i);
                            }
                            else if (id is WzStringProperty)
                            {
                                String i = ((WzStringProperty)id).Value;
                                ReactorData.Add(i);
                            }
                        }
                    }
                    
                }
                this.BWORK.ReportProgress(vv++ * 100 / listNode.Count, "正在蒐集MAP節點資訊 - " + node.Text);
                Thread.Sleep(1);
            }

            ret.Add(ObjData);
            ret.Add(BackData);
            ret.Add(TileData);
            ret.Add(NpcData);
            ret.Add(MobData);
            ret.Add(MapData);
            ret.Add(StringData);
            ret.Add(ReactorData);
            ret.Add(SoundData);
            
            return ret;
        }

        public WzFile getLowFile(String name)
        {
            foreach (WzFile file in LList)
            {
                if (file.Name == name)
                {
                    return file;
                }
            }
            return null;
        }



        public WzFile getHighFile(String name)
        {
            foreach (WzFile file in HList)
            {
                if (file.Name == name)
                {
                    return file;
                }
            }

            return null;
        }

        public WzNode FindHighNode(String path)
        {
            String[] data = path.Split(new char[] { '/' });
            WzFile file = getHighFile(data[0]); //wz目錄
            if (file == null) return null;
            WzDirectory dic = file.WzDirectory;
            for (int i = 1; i < data.Length - 1; i++)
            {
                dic = dic.GetDirectoryByName(data[i]);
                if (dic == null)
                {
                    return null;
                }
            }

            WzImage img = dic.GetImageByName(data[data.Length - 1] + ".img");
            if (img == null) return null;
            WzNode ret = new WzNode(img);
            return ret;
        }

        public WzDirectory FindLowNode(String path)
        {
            String[] data = path.Split(new char[] { '/' });
            WzFile file = getLowFile(data[0]); //wz目錄
            if (file == null) return null;
            WzDirectory dic = file.WzDirectory;
            for (int i = 1; i < data.Length - 1; i++)
            {
                dic = dic.GetDirectoryByName(data[i]);
                if (dic == null)
                {
                    return null;
                }
            }
            return dic;
        }

        public void modifyLowMob(WzNode node)
        {
            foreach (WzNode n in node.Nodes)
            {
                if (n.Text.Contains("attack"))
                {
                    WzImageProperty prop = (WzImageProperty)n.Tag;
                    WzImageProperty info = prop.GetProperty("info");
                    if (info != null)
                    {
                        WzImageProperty range = info.GetProperty("range");
                        if (range != null && range.GetProperty("sp") != null)
                        {
                            WzVectorProperty sp = (WzVectorProperty)range.GetProperty("sp");
                            WzIntProperty r = (WzIntProperty)range.GetProperty("r");
                            WzVectorProperty rb = new WzVectorProperty("rb", new WzIntProperty("X", Math.Abs(sp.X.Value)), new WzIntProperty("Y", Math.Abs(sp.Y.Value)));
                            WzVectorProperty It = new WzVectorProperty("lt", new WzIntProperty("X", Math.Abs(sp.X.Value) * -1), new WzIntProperty("Y", Math.Abs(sp.Y.Value) * -1));
                            ((WzSubProperty)range).AddProperty(rb);
                            ((WzSubProperty)range).AddProperty(It);
                            ((WzSubProperty)range).RemoveProperty(sp);
                            if (r != null) ((WzSubProperty)range).RemoveProperty(r);
                        }
                    }
                }
            }

            WzImage img = (WzImage)node.Tag;
            img.Changed = true;
        }


        public void addMobRevive(WzNode node, HashSet<String> str, HashSet<String> sound)
        {
            foreach (WzNode n in node.Nodes)
            {
                if (n.Text.Contains("info"))
                {
                    WzImageProperty revive = ((WzImageProperty)n.Tag).GetProperty("revive");
                    if (revive != null)
                    {
                        foreach (WzImageProperty m in revive.WzProperties)
                        {
                            String id;
                            if (m is WzStringProperty)
                            {
                                id = ((WzStringProperty)m).Value;
                            }
                            else
                            {
                                id = ((WzIntProperty)m).Value.ToString();
                            }

                            id += ".img";

                            WzDirectory mob = getLowFile("Mob.wz").WzDirectory;
                            String[] test = { "Mob.wz", "Mob2.wz", "Mob001.wz" };
                            if (mob.GetImageByName(id) != null) continue;

                            foreach (String wz in test)
                            {
                                WzDirectory Hmob = getHighFile(wz).WzDirectory;
                                WzImage linkadd = Hmob.GetImageByName(id);
                                if (linkadd == null) continue;

                                linkadd.ParseImage();
                                WzNode tt = new WzNode(linkadd);
                                ParseImg(tt);
                                modifyLowMob(tt);
                                String ss = id.Split(new char[] { '.' })[0];
                                str.Add("Mob.img/" + ss);
                                sound.Add("Mob/" + ss);
                                mob.AddImage(linkadd.DeepClone());
                                mob.GetImageByName(id).Changed = true;
                            }

                        }
                    }
                }
            }

            WzImage img = (WzImage)node.Tag;
            img.Changed = true;
        }

        public void findJsonNode(WzNode n)
        {

            Queue<WzNode> que = new Queue<WzNode>();
            WzImage img = (WzImage)n.Tag;
            foreach (WzImageProperty pp in img.WzProperties)
            {
                WzNode node = new WzNode(pp);
                que.Enqueue(node);
            }

            while (que.Count > 0)
            {
                WzNode prop = que.Dequeue();
                if (prop.Text.Contains(".") || (!((WzObject)prop.Tag is WzSubProperty) && prop.Text == "spine"))
                {
                    prop.DeleteWzNode();
                }
                else
                {

                    foreach (WzNode p in prop.Nodes)
                    {
                        que.Enqueue(p);
                    }

                }
            }
            ((WzImage)n.Tag).Changed = true;
        }


        public Tuple<HashSet<String>, HashSet<String>> getLowMapInfo()
        {
            HashSet<String> list = new HashSet<String>();
            HashSet<String> main = new HashSet<string>();
            WzDirectory file = getLowFile("Map.wz").WzDirectory;
            if (file == null) return new Tuple<HashSet<String>, HashSet<String>>(list, main);

            WzDirectory dic = file.GetDirectoryByName("Map");
            foreach (WzDirectory p in dic.WzDirectories)
            {
                foreach (WzImage img in p.WzImages)
                {
                    foreach (WzImageProperty ii in img.WzProperties)
                    {
                        main.Add(ii.Name);
                        if (ii.Name == "info")
                        {
                            foreach (WzImageProperty x in ii.WzProperties)
                            {
                                list.Add(x.Name);
                            }
                        }
                    }


                }
            }
            return new Tuple<HashSet<String>, HashSet<String>>(list, main);
        }



        public void modifyLowNpc(WzNode node, HashSet<String> str)
        {
            WzImage img = (WzImage)node.Tag;
            foreach (WzImageProperty prop in img.WzProperties.ToArray())
            {
                if (prop.Name.Contains("condition"))
                {
                    prop.Remove();
                }

                if (prop.Name == "info")
                {
                    foreach (WzImageProperty pt in prop.WzProperties.ToArray())
                    {
                        if (pt.Name == "link")
                        {
                            String id;
                            if (pt is WzStringProperty)
                            {
                                id = ((WzStringProperty)pt).Value;
                            }
                            else
                            {
                                id = ((WzIntProperty)pt).Value.ToString();
                            }

                            id += ".img";

                            WzDirectory npc = getLowFile("Npc.wz").WzDirectory;
                            WzDirectory Hnpc = getHighFile("Npc.wz").WzDirectory;
                            if (npc.GetImageByName(id) == null)
                            {
                                WzImage linkadd = Hnpc.GetImageByName(id);
                                if (linkadd != null)
                                {
                                    linkadd.ParseImage();
                                    WzNode tt = new WzNode(linkadd);
                                    ParseImg(tt);
                                    String ss = id.Split(new char[] { '.' })[0];
                                    str.Add("Npc.img/" + ss);
                                    modifyLowNpc(tt, str);
                                    npc.AddImage(linkadd.DeepClone());
                                    npc.GetImageByName(id).Changed = true;
                                }
                            }
                        }
                    }
                }
            }
            img.Changed = true;
        }


        public void AddMap(List<WzNode> listNode)
        {
            List<HashSet<String>> data = GetMapData(listNode);
            StringBuilder sb = new StringBuilder();
            StringBuilder error = new StringBuilder();


            //dealing map
            String[] obj = { "Map.wz", "Map2.wz" };
            String[] back = { "Map001.wz", "Map2.wz" };
            String tile = "Map.wz";
            String[] Mob = { "Mob.wz", "Mob2.wz", "Mob001.wz" };
            String[] Sound = { "Sound.wz", "Sound001.wz", "Sound2.wz" };

            HashSet<String> ObjIndex = data[0];
            int oa = 1;
            foreach (String s in ObjIndex)
            {
                WzNode n = null;
                Boolean found = false;
                foreach (String head in obj)
                {
                    n = FindHighNode(head + "/Obj/" + s);
                    if (n != null)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found || n == null)
                {
                    BWORK.ReportProgress(oa++ * 100 / ObjIndex.Count, "[錯誤訊息] : 並未找到物件 " + "Obj/" + s);
                    continue;
                }
                WzDirectory dic = FindLowNode(OBJADD.Text + "/Obj/" + s);
                if (dic == null)
                {
                    BWORK.ReportProgress(oa++ * 100 / ObjIndex.Count, "[錯誤訊息] : 並未找到 " + "Obj/" + s);
                    continue;
                }
                WzImage img = (WzImage)n.Tag;
                img.ParseImage();
                WzNode x = new WzNode(img);
                ParseImg(x);
                findJsonNode(x);
                if (dic.GetImageByName(img.Name) != null)
                {
                    WzNode rem = new WzNode(dic.GetImageByName(img.Name));
                    rem.DeleteWzNode();
                }
                dic.AddImage(img.DeepClone());
                dic.GetImageByName(img.Name).Changed = true;
                BWORK.ReportProgress(oa++ *100/ ObjIndex.Count, "正在同步Obj - " + s);
            }



            HashSet<String> BackIndex = data[1];

            int ba = 1;
            foreach (String s in BackIndex)
            {
                WzNode n = null;
                Boolean found = false;
                foreach (String head in back)
                {
                    n = FindHighNode(head + "/Back/" + s);
                    if (n != null)
                    {
                        found = true;
                        break;
                    }
                }
                if ((!found && s.Length > 0) || n == null)
                {
                    BWORK.ReportProgress(ba++ * 100 / BackIndex.Count, "[錯誤訊息] : 並未找到物件 " + "Back/" + s);
                    continue;
                }
                WzDirectory dic = FindLowNode(BACKADD.Text + "/Back/" + s);
                if (dic == null)
                {
                    BWORK.ReportProgress(ba++ * 100 / BackIndex.Count, "[錯誤訊息] : 並未找到 " + "Back/" + s);
                    continue;
                }
                WzImage img = (WzImage)n.Tag;
                img.ParseImage();
                WzNode x = new WzNode(img);
                ParseImg(x);
                findJsonNode(x);
                if (dic.GetImageByName(img.Name) != null)
                {
                    WzNode rem = new WzNode(dic.GetImageByName(img.Name));
                    rem.DeleteWzNode();
                }
                dic.AddImage(img.DeepClone());
                dic.GetImageByName(img.Name).Changed = true;
                BWORK.ReportProgress(ba++ * 100 / BackIndex.Count, "正在同步Back - " + s);
            }

            HashSet<String> TileIndex = data[2];
            int sa = 1;
            foreach (String s in TileIndex)
            {
                WzNode n = FindHighNode(tile + "/Tile/" + s);
                if (n == null)
                {
                    BWORK.ReportProgress(sa++ * 100 / TileIndex.Count, "[錯誤訊息] : 並未找到磚塊 " + "Tile/" + s);
                    continue;
                }
                WzDirectory dic = FindLowNode(TILEADD.Text + "/Tile/" + s);
                if (dic == null)
                {
                    BWORK.ReportProgress(sa++ * 100 / TileIndex.Count, "[錯誤訊息] : 並未找到 " + "Tile/" + s);
                    continue;
                }
                WzImage img = (WzImage)n.Tag;
                img.ParseImage();
                WzNode x = new WzNode(img);
                ParseImg(x);
                if (dic.GetImageByName(img.Name) != null)
                {
                    WzNode rem = new WzNode(dic.GetImageByName(img.Name));
                    rem.DeleteWzNode();
                }
                dic.AddImage(img.DeepClone());
                dic.GetImageByName(img.Name).Changed = true;
                BWORK.ReportProgress(sa++ * 100 / TileIndex.Count, "正在同步Tile - " + s);
            }

            HashSet<String> NpcIndex = data[3];
            int na = 1;
            foreach (String s in NpcIndex)
            {
                WzNode n = FindHighNode("Npc.wz/" + s);
                if (n == null)
                {
                    BWORK.ReportProgress(na++ * 100 / NpcIndex.Count, "[錯誤訊息] : 並未找到NPC " + s);
                    continue;
                }
                WzDirectory dic = FindLowNode("Npc.wz/" + s);
                if (dic == null)
                {
                    BWORK.ReportProgress(na++ * 100 / NpcIndex.Count, "[錯誤訊息] : 並未找到 " + "Npc.wz/" + s);
                    continue;
                }
                WzImage img = (WzImage)n.Tag;
                img.ParseImage();
                WzNode x = new WzNode(img);
                ParseImg(x);
                modifyLowNpc(x, data[6]);
                if (dic.GetImageByName(img.Name) == null)
                {
                    dic.AddImage(img.DeepClone());
                    dic.GetImageByName(img.Name).Changed = true;
                }
                BWORK.ReportProgress(na++ * 100 / NpcIndex.Count, "正在同步Npc - " + s);
            }

            HashSet<String> MobData = data[4];
            int ma = 1;
            foreach (String s in MobData)
            {
                Boolean found = false;
                WzNode n = null;
                foreach (String head in Mob)
                {
                    n = FindHighNode(head + "/" + s);
                    if (n != null)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found || n == null)
                {
                    BWORK.ReportProgress(ma++ * 100 / MobData.Count, "[錯誤訊息] : 並未找到怪物 " + s);
                    continue;
                }
                WzDirectory dic = FindLowNode("Mob.wz/" + s);
                if (dic == null)
                {
                    BWORK.ReportProgress(ma++ * 100 / MobData.Count, "[錯誤訊息] : 並未找到 " + "Mob.wz/" + s);
                    continue;
                }
                WzImage img = (WzImage)n.Tag;
                img.ParseImage();
                WzNode x = new WzNode(img);
                ParseImg(x);
                if (LList[0].Version < 145)
                {
                    modifyLowMob(x);
                }
                addMobRevive(x, data[6], data[8]); //加資料到stringdata
                if (dic.GetImageByName(img.Name) == null)
                {
                    dic.AddImage(img.DeepClone());
                    dic.GetImageByName(img.Name).Changed = true;
                }
                BWORK.ReportProgress(ma++ * 100 / MobData.Count, "正在同步Mob - " + s);
            }

            HashSet<String> MapData = data[5];
            Tuple<HashSet<String>, HashSet<String>> tu = getLowMapInfo();
            HashSet<String> list = tu.Item1;
            HashSet<String> main = tu.Item2;
            int mapa = 1;
            foreach (String s in MapData)
            {
                String index = (int.Parse(s) / 100000000).ToString();
                WzNode n = FindHighNode("Map002.wz/Map/Map" + index + "/" + s);
                if (n == null)
                {
                    BWORK.ReportProgress(mapa++ * 100 / MapData.Count, "[錯誤訊息] : 並未找到地圖 " + s);
                    continue;
                }
                WzImage img = (WzImage)n.Tag;
                img.ParseImage();
                foreach (WzImageProperty ii in img.WzProperties.ToArray())
                {
                    if (!main.Contains(ii.Name))
                    {
                        ii.Remove();
                    }
                    if (ii.Name == "info")
                    {
                        foreach (WzImageProperty px in ii.WzProperties.ToArray())
                        {
                            if (!list.Contains(px.Name))
                            {
                                px.Remove();
                            }
                        }
                    }
                }
                img.Changed = true;
                WzNode x = new WzNode(img);
                ParseImg(x);
                WzDirectory dic = getLowFile(MAPADD.Text).WzDirectory;
                WzDirectory dic2 = dic.GetDirectoryByName("Map");
                if (dic2 == null)
                {
                    BWORK.ReportProgress(mapa++ * 100 / MapData.Count, "[錯誤訊息] : 並未找到 " + "Map.wz/Map");
                    continue;
                }
                WzDirectory dic3 = dic2.GetDirectoryByName("Map" + index);
                if (dic3 == null)
                {
                    WzDirectory newDic = new WzDirectory("Map" + index, getLowFile("Map.wz"));
                    newDic.AddImage(img.DeepClone());
                    dic2.AddDirectory(newDic);
                }
                else
                {
                    dic3.AddImage(img.DeepClone());
                }
                BWORK.ReportProgress(mapa++ * 100 / MapData.Count, "正在同步Map - " + s);
            }

            //以下處理String
            WzFile HString = getHighFile("String.wz");
            WzFile LString = getLowFile("String.wz");
            HashSet<String> StringData = data[6];
            if (HString != null && LString != null)
            {
                int sta = 1;
                foreach (String s in StringData)
                {
                    String[] x = s.Split(new char[] { '/' });
                    WzImage Himg = HString.WzDirectory.GetImageByName(x[0]);
                    WzImage Limg = LString.WzDirectory.GetImageByName(x[0]);
                    if (Himg != null && Limg != null)
                    {
                        WzImageProperty prop = Himg.GetWzImageProperty(x[1]);
                        if (prop != null && Limg.GetWzImageProperty(x[1]) == null)
                        {
                            Limg.AddProperty(prop.DeepClone());
                            Limg.Changed = true;
                        }
                    }
                    BWORK.ReportProgress(sta++ * 100 / StringData.Count, "正在同步String - " + s);
                }
            }


            mark();

            //以下處理Reactor
            HashSet<String> ReactorData = data[7];
            int ra = 1;
            foreach (String s in ReactorData)
            {
                WzNode n = FindHighNode("Reactor.wz/" + s);
                if (n == null)
                {
                    BWORK.ReportProgress(ra++ * 100 / ReactorData.Count, "[錯誤訊息] : 並未找到反應堆 " + s);
                    continue;
                }

                WzDirectory dic = FindLowNode("Reactor.wz/" + s);
                if (dic == null)
                {
                    BWORK.ReportProgress(ra++ * 100 / ReactorData.Count, "[錯誤訊息] : 並未找到 " + "Reactor.wz/" + s);
                    continue;
                }
                WzImage img = (WzImage)n.Tag;
                img.ParseImage();
                WzNode x = new WzNode(img);
                ParseImg(x);
                if (dic.GetImageByName(img.Name) == null)
                {
                    dic.AddImage(img.DeepClone());
                    dic.GetImageByName(img.Name).Changed = true;
                }
                BWORK.ReportProgress(ra++ * 100 / ReactorData.Count, "正在同步Reactor - " + s);
            }


            HashSet<String> SoundData = data[8];
            int sda = 1;
            foreach (String s in SoundData)
            {
                WzFile ff = getLowFile("Sound.wz");
                Boolean found = false;
                if (ff != null)
                {
                    WzDirectory Lf = ff.WzDirectory;
                    String[] path = s.Split(new char[] { '/' });
                    foreach (String head in Sound)
                    {
                        WzDirectory Hf = getHighFile(head).WzDirectory;
                        if (Hf == null) continue;
                        WzImage img = Hf.GetImageByName(path[0] + ".img");
                        if (img == null) continue;
                        found = true;
                        WzImage LImg = Lf.GetImageByName(path[0] + ".img");
                        if (LImg == null)
                        {
                            Lf.AddImage(img.DeepClone());
                            Lf.GetImageByName(path[0] + ".img").Changed = true;
                            break;
                        }
                        else
                        {
                            WzImageProperty prop = img.GetWzImageProperty(path[1]);
                            if (prop == null) continue;
                            WzImageProperty Lprop = LImg.GetWzImageProperty(path[1]);
                            if (Lprop == null)
                            {
                                LImg.AddProperty(prop);
                                LImg.Changed = true;
                                break;
                            }
                        }
                    }
                }
                BWORK.ReportProgress(sda++ * 100 / SoundData.Count, "正在同步Sound - " + s);

            }
        }



        public void mark()
        {
            WzFile HString = getHighFile("String.wz");
            WzFile LString = getLowFile("String.wz");
            if (HString != null && LString != null)
            {
                WzImage mapStringImg = HString.WzDirectory.GetImageByName("Map.img");
                WzImage toolTipImg = HString.WzDirectory.GetImageByName("ToolTipHelp.img");
                if (mapStringImg != null)
                {
                    mapStringImg.ParseImage();
                    if (LString.WzDirectory.GetImageByName("Map.img") != null)
                    {
                        WzNode rem = new WzNode(LString.WzDirectory.GetImageByName("Map.img"));
                        rem.DeleteWzNode();
                    }
                    LString.WzDirectory.AddImage(mapStringImg);
                    LString.WzDirectory.GetImageByName("Map.img").Changed = true;

                }

                if (toolTipImg != null)
                {
                    toolTipImg.ParseImage();
                    WzImage i = LString.WzDirectory.GetImageByName("ToolTipHelp.img");
                    if (i != null)
                    {
                        //WzNode rem = new WzNode(LString.WzDirectory.GetImageByName("ToolTipHelp.img"));
                        //rem.DeleteWzNode();
                        WzImageProperty tool = i.GetWzImageProperty("Mapobject");
                        WzImageProperty toolH = toolTipImg.GetWzImageProperty("Mapobject");
                        if (tool != null && toolH != null)
                        {
                            foreach (WzImageProperty tip in toolH.WzProperties)
                            {
                                if (tool.GetProperty(tip.Name) == null)
                                {
                                    ((WzSubProperty)tool).AddProperty(tip.DeepClone());
                                }

                            }
                        }
                    }
                    //LString.WzDirectory.AddImage(toolTipImg);
                    i.Changed = true;
                }
            }



            WzFile Hmark = getHighFile("Map.wz");
            WzFile Lmark = getLowFile("Map.wz");
            if (Hmark != null && Lmark != null)
            {
                WzImage HImg = Hmark.WzDirectory.GetImageByName("MapHelper.img");
                WzImage LImg = Lmark.WzDirectory.GetImageByName("MapHelper.img");
                if (HImg != null && LImg != null)
                {
                    WzImageProperty HMMark = HImg.GetWzImageProperty("mark");
                    if (HMMark != null)
                    {
                        WzNode o = new WzNode(HMMark);
                        ParseNode(o);
                        if (LImg.GetWzImageProperty("mark") != null)
                        {
                            LImg.RemoveProperty(LImg.GetWzImageProperty("mark"));
                        }

                        LImg.AddProperty(HMMark);
                        LImg.Changed = true;
                    }
                }
            }
        }

        private void LOWMAP_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            dialog.Description = "請打開低版本主程式";

            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            LList = new List<WzFile>();
            openWz(LList, dialog, WzMapleVersion.EMS);
            this.LOWPATH.Text = dialog.SelectedPath;
        }

        private void HIGHMAP_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            dialog.Description = "請打開高版本主程式";

            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            HList = new List<WzFile>();
            openWz(HList, dialog, WzMapleVersion.BMS);
            this.HIGHPATH.Text = dialog.SelectedPath;
        }

        private void ADD_Click(object sender, EventArgs e)
        {
            this.BWORK.RunWorkerAsync();
        }

        private void BWORK_DoWork(object sender, DoWorkEventArgs e)
        {
            List<WzNode> nodes = new List<WzNode>();
            WzFile f = getHighFile("Map002.wz");
            if (f == null)
            {
                MessageBox.Show("高版本主程式無載入Map002.wz");
                return;
            }
            WzDirectory dic = f.WzDirectory.GetDirectoryByName("Map");
            if (dic == null)
            {
                MessageBox.Show("高版本主程式無載入Map002.wz/Map");
                return;
            }
            String start = STARTMAP.Text;
            String end = ENDMAP.Text;
            int num = 0;

            if (start[0] != end[0])
            {
                MessageBox.Show("填寫地圖區間過大,請重新填寫");
                return;
            }

            if (!int.TryParse(start, out num) || !int.TryParse(end, out num))
            {
                MessageBox.Show("填寫地圖區間僅限數字,請重新填寫");
                return;
            }


            int s = int.Parse(start);
            int en = int.Parse(end);

            if( s >= en)
            {
                MessageBox.Show("開始地圖需比結束地圖還小");
                return;
            }

            WzDirectory certain = dic.GetDirectoryByName("Map" + start[0]);
            if (certain == null)
            {
                MessageBox.Show("高版本主程式無載入Map002.wz/Map/Map" + start[0]);
                return;
            }


            foreach (WzImage img in certain.WzImages)
            {
                int map = int.Parse(img.Name.Split(new char[] { '.' })[0]);
                if (map >= s && map <= en)
                {
                    WzNode n = new WzNode(img);
                    nodes.Add(n);
                }
            }

            AddMap(nodes);
        }

        private void BWORK_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PBAR.Value = e.ProgressPercentage;
            String message = e.UserState.ToString();
            this.progress.Text = string.Format(message + " 目前進度...{0}%", e.ProgressPercentage);
            PBAR.Update();
        }

        private void SAVE_Click(object sender, EventArgs e)
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
            Close();
        }

        private void BWORK_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("已完成同步程序");
        }


    }
}
