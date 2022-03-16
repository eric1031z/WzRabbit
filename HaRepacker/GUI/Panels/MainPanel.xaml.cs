using HaRepacker.Comparer;
using HaRepacker.Converter;
using HaRepacker.GUI.Input;
using HaSharedLibrary.Render.DX;
using HaSharedLibrary.GUI;
using HaSharedLibrary.Util;
using MapleLib.WzLib;
using MapleLib.WzLib.Spine;
using MapleLib.WzLib.WzProperties;
using MapleLib.Converters;
using Microsoft.Xna.Framework;
using Spine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static MapleLib.Configuration.UserSettings;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;
using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Color = System.Drawing.Color;

namespace HaRepacker.GUI.Panels
{
    /// <summary>
    /// Interaction logic for MainPanelXAML.xaml
    /// </summary>
    public partial class MainPanel : UserControl
    {
        // Constants
        private const string FIELD_LIMIT_OBJ_NAME = "fieldLimit";
        private const string FIELD_TYPE_OBJ_NAME = "fieldType";
        private const string PORTAL_NAME_OBJ_NAME = "pn";

        // Etc
        private static List<WzObject> clipboard = new List<WzObject>();
        private UndoRedoManager undoRedoMan;

        private bool isSelectingWzMapFieldLimit = false;
        private bool isLoading = false;

        public MainPanel()
        {
            InitializeComponent();

            isLoading = true;

            // undo redo
            undoRedoMan = new UndoRedoManager(this);

            // Set theme color
            if (Program.ConfigurationManager.UserSettings.ThemeColor == (int)UserSettingsThemeColor.Dark)
            {
                VisualStateManager.GoToState(this, "BlackTheme", false);
                DataTree.BackColor = System.Drawing.Color.Black;
                DataTree.ForeColor = System.Drawing.Color.White;
            }

            nameBox.Header = "Name";
            textPropBox.Header = "Value";
            textPropBox.ButtonClicked += applyChangesButton_Click;

            vectorPanel.ButtonClicked += VectorPanel_ButtonClicked;

            textPropBox.Visibility = Visibility.Collapsed;
            //nameBox.Visibility = Visibility.Collapsed;

            // Storyboard
            System.Windows.Media.Animation.Storyboard sbb = (System.Windows.Media.Animation.Storyboard)(this.FindResource("Storyboard_Find_FadeIn"));
            sbb.Completed += Storyboard_Find_FadeIn_Completed;


            // buttons
            menuItem_Animate.Visibility = Visibility.Collapsed;
            menuItem_changeImage.Visibility = Visibility.Collapsed;
            menuItem_changeSound.Visibility = Visibility.Collapsed;
            menuItem_saveSound.Visibility = Visibility.Collapsed;
            menuItem_saveImage.Visibility = Visibility.Collapsed;

            textEditor.SaveButtonClicked += TextEditor_SaveButtonClicked;
            Loaded += MainPanelXAML_Loaded;


            isLoading = false;
        }

        private void MainPanelXAML_Loaded(object sender, RoutedEventArgs e)
        {
            this.fieldLimitPanel1.SetTextboxOnFieldLimitChange(textPropBox);
            this.fieldTypePanel.SetTextboxOnFieldTypeChange(textPropBox);
        }

        #region Exported Fields
        public UndoRedoManager UndoRedoMan { get { return undoRedoMan; } }

        #endregion

        #region Data Tree
        private void DataTree_DoubleClick(object sender, EventArgs e)
        {
            if (DataTree.SelectedNode != null && DataTree.SelectedNode.Tag is WzImage && DataTree.SelectedNode.Nodes.Count == 0)
            {
                ParseOnDataTreeSelectedItem(((WzNode)DataTree.SelectedNode), true);
            }
        }

        private void DataTree_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            if (DataTree.SelectedNode == null)
            {
                return;
            }

            ShowObjectValue((WzObject)DataTree.SelectedNode.Tag);
            selectionLabel.Text = string.Format(Properties.Resources.SelectionType, ((WzNode)DataTree.SelectedNode).GetTypeName());
        }

        /// <summary>
        /// Parse the data tree selected item on double clicking, or copy pasting into it.
        /// </summary>
        /// <param name="selectedNode"></param>
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

        private void DataTree_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (!DataTree.Focused) return;
            bool ctrl = (System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Control) == System.Windows.Forms.Keys.Control;
            bool alt = (System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Alt) == System.Windows.Forms.Keys.Alt;
            bool shift = (System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift;
            System.Windows.Forms.Keys filteredKeys = e.KeyData;
            if (ctrl) filteredKeys = filteredKeys ^ System.Windows.Forms.Keys.Control;
            if (alt) filteredKeys = filteredKeys ^ System.Windows.Forms.Keys.Alt;
            if (shift) filteredKeys = filteredKeys ^ System.Windows.Forms.Keys.Shift;

            switch (filteredKeys)
            {
                case System.Windows.Forms.Keys.F5:
                    StartAnimateSelectedCanvas();
                    break;
                case System.Windows.Forms.Keys.Escape:
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;

                case System.Windows.Forms.Keys.Delete:
                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    PromptRemoveSelectedTreeNodes();
                    break;
            }
            if (ctrl)
            {
                switch (filteredKeys)
                {
                    case System.Windows.Forms.Keys.R: // Render map        
                        //HaRepackerMainPanel.

                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                    case System.Windows.Forms.Keys.C:
                        DoCopy();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                    case System.Windows.Forms.Keys.V:
                        DoPaste();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                    case System.Windows.Forms.Keys.F: // open search box
                        if (grid_FindPanel.Visibility == Visibility.Collapsed)
                        {
                            System.Windows.Media.Animation.Storyboard sbb = (System.Windows.Media.Animation.Storyboard)(this.FindResource("Storyboard_Find_FadeIn"));
                            sbb.Begin();

                            e.Handled = true;
                            e.SuppressKeyPress = true;
                        }
                        break;
                    case System.Windows.Forms.Keys.T:
                    case System.Windows.Forms.Keys.O:
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                }
            }
        }
        #endregion

        #region Image directory add
        /// <summary>
        /// WzDirectory
        /// </summary>
        /// <param name="target"></param>
        public void AddWzDirectoryToSelectedNode(System.Windows.Forms.TreeNode target)
        {
            if (!(target.Tag is WzDirectory) && !(target.Tag is WzFile))
            {
                Warning.Error(Properties.Resources.MainCannotInsertToNode);
                return;
            }
            string name;
            if (!NameInputBox.Show(Properties.Resources.MainAddDir, 0, out name))
                return;

            bool added = false;

            WzObject obj = (WzObject)target.Tag;
            while (obj is WzFile || ((obj = obj.Parent) is WzFile))
            {
                WzFile topMostWzFileParent = (WzFile)obj;

                ((WzNode)target).AddObject(new WzDirectory(name, topMostWzFileParent), UndoRedoMan);
                added = true;
                break;
            }
            if (!added)
            {
                MessageBox.Show(Properties.Resources.MainTreeAddDirError);
            }
        }

        /// <summary>
        /// WzDirectory
        /// </summary>
        /// <param name="target"></param>
        public void AddWzImageToSelectedNode(System.Windows.Forms.TreeNode target)
        {
            string name;
            if (!(target.Tag is WzDirectory) && !(target.Tag is WzFile))
            {
                Warning.Error(Properties.Resources.MainCannotInsertToNode);
                return;
            }
            else if (!NameInputBox.Show(Properties.Resources.MainAddImg, 0, out name))
                return;
            ((WzNode)target).AddObject(new WzImage(name) { Changed = true }, UndoRedoMan);
        }

        /// <summary>
        /// WzByteProperty
        /// </summary>
        /// <param name="target"></param>
        public void AddWzByteFloatToSelectedNode(System.Windows.Forms.TreeNode target)
        {
            string name;
            double? d;
            if (!(target.Tag is IPropertyContainer))
            {
                Warning.Error(Properties.Resources.MainCannotInsertToNode);
                return;
            }
            else if (!FloatingPointInputBox.Show(Properties.Resources.MainAddFloat, out name, out d))
                return;
            ((WzNode)target).AddObject(new WzFloatProperty(name, (float)d), UndoRedoMan);
        }

        /// <summary>
        /// WzCanvasProperty
        /// </summary>
        /// <param name="target"></param>
        public void AddWzCanvasToSelectedNode(System.Windows.Forms.TreeNode target)
        {
            string name;
            List<System.Drawing.Bitmap> bitmaps = new List<System.Drawing.Bitmap>();
            if (!(target.Tag is IPropertyContainer))
            {
                Warning.Error(Properties.Resources.MainCannotInsertToNode);
                return;
            }
            else if (!BitmapInputBox.Show(Properties.Resources.MainAddCanvas, out name, out bitmaps))
                return;

            WzNode wzNode = ((WzNode)target);

            int i = 0;
            foreach (System.Drawing.Bitmap bmp in bitmaps)
            {
                WzCanvasProperty canvas = new WzCanvasProperty(bitmaps.Count == 1 ? name : (name + i));
                WzPngProperty pngProperty = new WzPngProperty();
                pngProperty.SetImage(bmp);
                canvas.PngProperty = pngProperty;

                WzNode newInsertedNode = wzNode.AddObject(canvas, UndoRedoMan);
                // Add an additional WzVectorProperty with X Y of 0,0
                newInsertedNode.AddObject(new WzVectorProperty(WzCanvasProperty.OriginPropertyName, new WzIntProperty("X", 0), new WzIntProperty("Y", 0)), UndoRedoMan);

                i++;
            }
        }

        /// <summary>
        /// WzCompressedInt
        /// </summary>
        /// <param name="target"></param>
        public void AddWzCompressedIntToSelectedNode(System.Windows.Forms.TreeNode target)
        {
            string name;
            int? value;
            if (!(target.Tag is IPropertyContainer))
            {
                Warning.Error(Properties.Resources.MainCannotInsertToNode);
                return;
            }
            else if (!IntInputBox.Show(
                Properties.Resources.MainAddInt,
                "", 0,
                out name, out value))
                return;
            ((WzNode)target).AddObject(new WzIntProperty(name, (int)value), UndoRedoMan);
        }

        /// <summary>
        /// WzLongProperty
        /// </summary>
        /// <param name="target"></param>
        public void AddWzLongToSelectedNode(System.Windows.Forms.TreeNode target)
        {
            string name;
            long? value;
            if (!(target.Tag is IPropertyContainer))
            {
                Warning.Error(Properties.Resources.MainCannotInsertToNode);
                return;
            }
            else if (!LongInputBox.Show(Properties.Resources.MainAddInt, out name, out value))
                return;
            ((WzNode)target).AddObject(new WzLongProperty(name, (long)value), UndoRedoMan);
        }

        /// <summary>
        /// WzConvexProperty
        /// </summary>
        /// <param name="target"></param>
        public void AddWzConvexPropertyToSelectedNode(System.Windows.Forms.TreeNode target)
        {
            string name;
            if (!(target.Tag is IPropertyContainer))
            {
                Warning.Error(Properties.Resources.MainCannotInsertToNode);
                return;
            }
            else if (!NameInputBox.Show(Properties.Resources.MainAddConvex, 0, out name))
                return;
            ((WzNode)target).AddObject(new WzConvexProperty(name), UndoRedoMan);
        }

        /// <summary>
        /// WzNullProperty
        /// </summary>
        /// <param name="target"></param>
        public void AddWzDoublePropertyToSelectedNode(System.Windows.Forms.TreeNode target)
        {
            string name;
            double? d;
            if (!(target.Tag is IPropertyContainer))
            {
                Warning.Error(Properties.Resources.MainCannotInsertToNode);
                return;
            }
            else if (!FloatingPointInputBox.Show(Properties.Resources.MainAddDouble, out name, out d))
                return;
            ((WzNode)target).AddObject(new WzDoubleProperty(name, (double)d), UndoRedoMan);
        }

        /// <summary>
        /// WzNullProperty
        /// </summary>
        /// <param name="target"></param>
        public void AddWzNullPropertyToSelectedNode(System.Windows.Forms.TreeNode target)
        {
            string name;
            if (!(target.Tag is IPropertyContainer))
            {
                Warning.Error(Properties.Resources.MainCannotInsertToNode);
                return;
            }
            else if (!NameInputBox.Show(Properties.Resources.MainAddNull, 0, out name))
                return;
            ((WzNode)target).AddObject(new WzNullProperty(name), UndoRedoMan);
        }

        /// <summary>
        /// WzSoundProperty
        /// </summary>
        /// <param name="target"></param>
        public void AddWzSoundPropertyToSelectedNode(System.Windows.Forms.TreeNode target)
        {
            string name;
            string path;
            if (!(target.Tag is IPropertyContainer))
            {
                Warning.Error(Properties.Resources.MainCannotInsertToNode);
                return;
            }
            else if (!SoundInputBox.Show(Properties.Resources.MainAddSound, out name, out path))
                return;
            ((WzNode)target).AddObject(new WzBinaryProperty(name, path), UndoRedoMan);
        }

        /// <summary>
        /// WzStringProperty
        /// </summary>
        /// <param name="target"></param>
        public void AddWzStringPropertyToSelectedIndex(System.Windows.Forms.TreeNode target)
        {
            string name;
            string value;
            if (!(target.Tag is IPropertyContainer))
            {
                Warning.Error(Properties.Resources.MainCannotInsertToNode);
                return;
            }
            else if (!NameValueInputBox.Show(Properties.Resources.MainAddString, out name, out value))
                return;
            ((WzNode)target).AddObject(new WzStringProperty(name, value), UndoRedoMan);
        }

        /// <summary>
        /// WzSubProperty
        /// </summary>
        /// <param name="target"></param>
        public void AddWzSubPropertyToSelectedIndex(System.Windows.Forms.TreeNode target)
        {
            string name;
            if (!(target.Tag is IPropertyContainer))
            {
                Warning.Error(Properties.Resources.MainCannotInsertToNode);
                return;
            }
            else if (!NameInputBox.Show(Properties.Resources.MainAddSub, 0, out name))
                return;
            ((WzNode)target).AddObject(new WzSubProperty(name), UndoRedoMan);
        }

        /// <summary>
        /// WzUnsignedShortProperty
        /// </summary>
        /// <param name="target"></param>
        public void AddWzUnsignedShortPropertyToSelectedIndex(System.Windows.Forms.TreeNode target)
        {
            string name;
            int? value;
            if (!(target.Tag is IPropertyContainer))
            {
                Warning.Error(Properties.Resources.MainCannotInsertToNode);
                return;
            }
            else if (!IntInputBox.Show(Properties.Resources.MainAddShort,
                "", 0,
                out name, out value))
                return;
            ((WzNode)target).AddObject(new WzShortProperty(name, (short)value), UndoRedoMan);
        }

        /// <summary>
        /// WzUOLProperty
        /// </summary>
        /// <param name="target"></param>
        public void AddWzUOLPropertyToSelectedIndex(System.Windows.Forms.TreeNode target)
        {
            string name;
            string value;
            if (!(target.Tag is IPropertyContainer))
            {
                Warning.Error(Properties.Resources.MainCannotInsertToNode);
                return;
            }
            else if (!NameValueInputBox.Show(Properties.Resources.MainAddLink, out name, out value))
                return;
            ((WzNode)target).AddObject(new WzUOLProperty(name, value), UndoRedoMan);
        }

        /// <summary>
        /// WzVectorProperty
        /// </summary>
        /// <param name="target"></param>
        public void AddWzVectorPropertyToSelectedIndex(System.Windows.Forms.TreeNode target)
        {
            string name;
            System.Drawing.Point? pt;
            if (!(target.Tag is IPropertyContainer))
            {
                Warning.Error(Properties.Resources.MainCannotInsertToNode);
                return;
            }
            else if (!VectorInputBox.Show(Properties.Resources.MainAddVec, out name, out pt))
                return;
            ((WzNode)target).AddObject(new WzVectorProperty(name, new WzIntProperty("X", ((System.Drawing.Point)pt).X), new WzIntProperty("Y", ((System.Drawing.Point)pt).Y)), UndoRedoMan);
        }

        /// <summary>
        /// WzLuaProperty
        /// </summary>
        /// <param name="target"></param>
        public void AddWzLuaPropertyToSelectedIndex(System.Windows.Forms.TreeNode target)
        {
 /*           string name;
            string value;
            if (!(target.Tag is WzDirectory) && !(target.Tag is WzFile))
            {
                Warning.Error(Properties.Resources.MainCannotInsertToNode);
                return;
            }
            else if (!NameValueInputBox.Show(Properties.Resources.MainAddString, out name, out value))
                return;

            string propertyName = name;
            if (!propertyName.EndsWith(".lua"))
            {
                propertyName += ".lua"; // it must end with .lua regardless
            }
            ((WzNode)target).AddObject(new WzImage(propertyName), UndoRedoMan);*/
        }

        /// <summary>
        /// Remove selected nodes
        /// </summary>
        public void PromptRemoveSelectedTreeNodes()
        {
            if (!Warning.Warn(Properties.Resources.MainConfirmRemove))
            {
                return;
            }

            List<UndoRedoAction> actions = new List<UndoRedoAction>();

            System.Windows.Forms.TreeNode[] nodeArr = new System.Windows.Forms.TreeNode[DataTree.SelectedNodes.Count];
            DataTree.SelectedNodes.CopyTo(nodeArr, 0);

            foreach (WzNode node in nodeArr)
                if (!(node.Tag is WzFile) && node.Parent != null)
                {
                    actions.Add(UndoRedoManager.ObjectRemoved((WzNode)node.Parent, node));
                    node.DeleteWzNode();
                }
            UndoRedoMan.AddUndoBatch(actions);
        }

        /// <summary>
        /// Rename an individual node
        /// </summary>
        public void PromptRenameWzTreeNode(WzNode node)
        {
            if (node == null)
                return;

            string newName = "";
            WzNode wzNode = node;
            if (RenameInputBox.Show(Properties.Resources.MainConfirmRename, wzNode.Text, out newName))
            {
                wzNode.ChangeName(newName);
            }
        }
        #endregion

        #region Panel Loading Events
        /// <summary>
        /// Set panel loading splash screen from MainForm.cs
        /// <paramref name="currentDispatcher"/>
        /// </summary>
        public void OnSetPanelLoading(Dispatcher currentDispatcher = null)
        {
            Action action = () =>
            {
                loadingPanel.OnStartAnimate();
                grid_LoadingPanel.Visibility = Visibility.Visible;
                treeView_WinFormsHost.Visibility = Visibility.Collapsed;
            };
            if (currentDispatcher != null)
                currentDispatcher.BeginInvoke(action);
            else
                grid_LoadingPanel.Dispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// Remove panel loading splash screen from MainForm.cs
        /// <paramref name="currentDispatcher"/>
        /// </summary>
        public void OnSetPanelLoadingCompleted(Dispatcher currentDispatcher = null)
        {
            Action action = () =>
            {
                loadingPanel.OnPauseAnimate();
                grid_LoadingPanel.Visibility = Visibility.Collapsed;
                treeView_WinFormsHost.Visibility = Visibility.Visible;
            };
            if (currentDispatcher != null)
                currentDispatcher.BeginInvoke(action);
            else
                grid_LoadingPanel.Dispatcher.BeginInvoke(action);
        }
        #endregion

        #region Animate
        /// <summary>
        /// Animate the list of selected canvases
        /// </summary>
        public void StartAnimateSelectedCanvas()
        {
            if (DataTree.SelectedNodes.Count == 0)
            {
                MessageBox.Show("Please select at least one or more canvas node.");
                return;
            }

            List<WzNode> selectedNodes = new List<WzNode>();
            foreach (WzNode node in DataTree.SelectedNodes)
            {
                selectedNodes.Add(node);
            }

            string path_title = ((WzNode)DataTree.SelectedNodes[0]).Parent?.FullPath ?? "Animate";

            Thread thread = new Thread(() =>
            {
                try
                {
                    ImageAnimationPreviewWindow previewWnd = new ImageAnimationPreviewWindow(selectedNodes, path_title);
                    previewWnd.Run();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error previewing animation. " + ex.ToString());
                }
            });
            thread.Start();
            // thread.Join();
        }

        private void nextLoopTime_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            /* if (nextLoopTime_comboBox == null)
                  return;

              switch (nextLoopTime_comboBox.SelectedIndex)
              {
                  case 1:
                      Program.ConfigurationManager.UserSettings.DelayNextLoop = 1000;
                      break;
                  case 2:
                      Program.ConfigurationManager.UserSettings.DelayNextLoop = 2000;
                      break;
                  case 3:
                      Program.ConfigurationManager.UserSettings.DelayNextLoop = 5000;
                      break;
                  case 4:
                      Program.ConfigurationManager.UserSettings.DelayNextLoop = 10000;
                      break;
                  default:
                      Program.ConfigurationManager.UserSettings.DelayNextLoop = Program.TimeStartAnimateDefault;
                      break;
              }*/
        }
        #endregion

        #region Buttons
        private void nameBox_ButtonClicked(object sender, EventArgs e)
        {
            if (DataTree.SelectedNode == null) return;
            if (DataTree.SelectedNode.Tag is WzFile)
            {
                ((WzFile)DataTree.SelectedNode.Tag).Header.Copyright = nameBox.Text;
                ((WzFile)DataTree.SelectedNode.Tag).Header.RecalculateFileStart();
            }
            else if (WzNode.CanNodeBeInserted((WzNode)DataTree.SelectedNode.Parent, nameBox.Text))
            {
                string text = nameBox.Text;
                ((WzNode)DataTree.SelectedNode).ChangeName(text);
                nameBox.Text = text;
                nameBox.ApplyButtonEnabled = false;
            }
            else
                Warning.Error(Properties.Resources.MainNodeExists);

        }

        /// <summary>
        /// On vector panel 'apply' button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VectorPanel_ButtonClicked(object sender, EventArgs e)
        {
            applyChangesButton_Click(null, null);
        }

        private void applyChangesButton_Click(object sender, EventArgs e)
        {
            if (DataTree.SelectedNode == null)
                return;

            string setText = textPropBox.Text;

            WzObject obj = (WzObject)DataTree.SelectedNode.Tag;
            if (obj is WzImageProperty imageProperty)
            {
                imageProperty.ParentImage.Changed = true;
            }

            if (obj is WzVectorProperty vectorProperty)
            {
                vectorProperty.X.Value = vectorPanel.X;
                vectorProperty.Y.Value = vectorPanel.Y;
            }
            else if (obj is WzStringProperty stringProperty)
            {
                if (!stringProperty.IsSpineAtlasResources)
                {
                    stringProperty.Value = setText;
                } else
                {
                    throw new NotSupportedException("Usage of textBoxProp for spine WzStringProperty.");
                }
            }
            else if (obj is WzFloatProperty floatProperty)
            {
                float val;
                if (!float.TryParse(setText, out val))
                {
                    Warning.Error(string.Format(Properties.Resources.MainConversionError, setText));
                    return;
                }
                floatProperty.Value = val;
            }
            else if (obj is WzIntProperty intProperty)
            {
                int val;
                if (!int.TryParse(setText, out val))
                {
                    Warning.Error(string.Format(Properties.Resources.MainConversionError, setText));
                    return;
                }
                intProperty.Value = val;
            }
            else if (obj is WzLongProperty longProperty)
            {
                long val;
                if (!long.TryParse(setText, out val))
                {
                    Warning.Error(string.Format(Properties.Resources.MainConversionError, setText));
                    return;
                }
                longProperty.Value = val;
            }
            else if (obj is WzDoubleProperty doubleProperty)
            {
                double val;
                if (!double.TryParse(setText, out val))
                {
                    Warning.Error(string.Format(Properties.Resources.MainConversionError, setText));
                    return;
                }
                doubleProperty.Value = val;
            }
            else if (obj is WzShortProperty shortProperty)
            {
                short val;
                if (!short.TryParse(setText, out val))
                {
                    Warning.Error(string.Format(Properties.Resources.MainConversionError, setText));
                    return;
                }
                shortProperty.Value = val;
            }
            else if (obj is WzUOLProperty UOLProperty)
            {
                UOLProperty.Value = setText;
            }
            else if (obj is WzLuaProperty)
            {
                throw new NotSupportedException("Moved to TextEditor_SaveButtonClicked()");
            }
        }

        /// <summary>
        /// On texteditor save button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextEditor_SaveButtonClicked(object sender, EventArgs e)
        {
            if (DataTree.SelectedNode == null)
                return;

            WzObject obj = (WzObject)DataTree.SelectedNode.Tag;
            if (obj is WzLuaProperty luaProp)
            {
                string setText = textEditor.textEditor.Text;
                byte[] encBytes = luaProp.EncodeDecode(Encoding.ASCII.GetBytes(setText));
                luaProp.Value = encBytes;
            } 
            else if (obj is WzStringProperty stringProp)
            {
                //if (stringProp.IsSpineAtlasResources)
               // {
                    string setText = textEditor.textEditor.Text;

                    stringProp.Value = setText;
              /*  } 
                else
                {
                    throw new NotSupportedException("Usage of TextEditor for non-spine WzStringProperty.");
                }*/
            }
        }

        /// <summary>
        /// More option -- Shows ContextMenuStrip 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_MoreOption_Click(object sender, RoutedEventArgs e)
        {
            Button clickSrc = (Button)sender;

            clickSrc.ContextMenu.IsOpen = true;
            //  System.Windows.Forms.ContextMenuStrip contextMenu = new System.Windows.Forms.ContextMenuStrip();
            //  contextMenu.Show(clickSrc, 0, 0);
        }

        /// <summary>
        /// Menu item for animation. Appears when clicking on the "..." button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Animate_Click(object sender, RoutedEventArgs e)
        {
            StartAnimateSelectedCanvas();
        }

        /// <summary>
        /// Save the image animation into a JPG file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_saveImageAnimation_Click(object sender, RoutedEventArgs e)
        {
            WzObject seletedWzObject = (WzObject)DataTree.SelectedNode.Tag;

            if (!AnimationBuilder.IsValidAnimationWzObject(seletedWzObject))
                return;

            // Check executing process architecture
            /*AssemblyName executingAssemblyName = Assembly.GetExecutingAssembly().GetName();
            var assemblyArchitecture = executingAssemblyName.ProcessorArchitecture;
            if (assemblyArchitecture == ProcessorArchitecture.None)
            {
                System.Windows.Forms.MessageBox.Show(HaRepacker.Properties.Resources.ExecutingAssemblyError, HaRepacker.Properties.Resources.Warning, System.Windows.Forms.MessageBoxButtons.OK);
                return;
            }*/

            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog()
            {
                Title = HaRepacker.Properties.Resources.SelectOutApng,
                Filter = string.Format("{0}|*.png", HaRepacker.Properties.Resources.ApngFilter)
            };
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            AnimationBuilder.ExtractAnimation((WzSubProperty)seletedWzObject, dialog.FileName, Program.ConfigurationManager.UserSettings.UseApngIncompatibilityFrame);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_changeImage_Click(object sender, RoutedEventArgs e)
        {
            if (DataTree.SelectedNode.Tag is WzCanvasProperty) // only allow button click if its an image property
            {
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog()
                {
                    Title = "Select an image",
                    Filter = "Supported Image Formats (*.png;*.bmp;*.jpg;*.gif;*.jpeg;*.tif;*.tiff)|*.png;*.bmp;*.jpg;*.gif;*.jpeg;*.tif;*.tiff"
                };
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                System.Drawing.Bitmap bmp;
                try
                {
                    bmp = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(dialog.FileName);
                }
                catch
                {
                    Warning.Error(Properties.Resources.MainImageLoadError);
                    return;
                }
                //List<UndoRedoAction> actions = new List<UndoRedoAction>(); // Undo action

                ChangeCanvasPropBoxImage(bmp);
            }
        }

        /// <summary>
        /// Changes the displayed image in 'canvasPropBox' with a user defined input.
        /// </summary>
        /// <param name="image"></param>
        /// <param name=""></param>
        private void ChangeCanvasPropBoxImage(Bitmap bmp)
        {
            if (DataTree.SelectedNode.Tag is WzCanvasProperty property)
            {
                WzCanvasProperty selectedWzCanvas = property;

                if (selectedWzCanvas.HaveInlinkProperty()) // if its an inlink property, remove that before updating base image.
                {
                    selectedWzCanvas.RemoveProperty(selectedWzCanvas[WzCanvasProperty.InlinkPropertyName]);

                    WzNode parentCanvasNode = (WzNode)DataTree.SelectedNode;
                    WzNode childInlinkNode = WzNode.GetChildNode(parentCanvasNode, WzCanvasProperty.InlinkPropertyName);

                    // Add undo actions
                    //actions.Add(UndoRedoManager.ObjectRemoved((WzNode)parentCanvasNode, childInlinkNode));
                    childInlinkNode.DeleteWzNode(); // Delete '_inlink' node

                    // TODO: changing _Inlink image crashes
                    // Mob2.wz/9400121/hit/0
                }
                else if (selectedWzCanvas.HaveOutlinkProperty()) // if its an inlink property, remove that before updating base image.
                {
                    selectedWzCanvas.RemoveProperty(selectedWzCanvas[WzCanvasProperty.OutlinkPropertyName]);

                    WzNode parentCanvasNode = (WzNode)DataTree.SelectedNode;
                    WzNode childInlinkNode = WzNode.GetChildNode(parentCanvasNode, WzCanvasProperty.OutlinkPropertyName);

                    // Add undo actions
                    //actions.Add(UndoRedoManager.ObjectRemoved((WzNode)parentCanvasNode, childInlinkNode));
                    childInlinkNode.DeleteWzNode(); // Delete '_inlink' node
                }

                selectedWzCanvas.PngProperty.SetImage(bmp);

                // Updates
                selectedWzCanvas.ParentImage.Changed = true;

                canvasPropBox.Image = bmp.ToWpfBitmap();

                // Add undo actions
                //UndoRedoMan.AddUndoBatch(actions);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_changeSound_Click(object sender, RoutedEventArgs e)
        {
            if (DataTree.SelectedNode.Tag is WzBinaryProperty)
            {
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog()
                {
                    Title = "Select the sound",
                    Filter = "Moving Pictures Experts Group Format 1 Audio Layer 3(*.mp3)|*.mp3"
                };
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                WzBinaryProperty prop;
                try
                {
                    prop = new WzBinaryProperty(((WzBinaryProperty)DataTree.SelectedNode.Tag).Name, dialog.FileName);
                }
                catch
                {
                    Warning.Error(Properties.Resources.MainImageLoadError);
                    return;
                }
                IPropertyContainer parent = (IPropertyContainer)((WzBinaryProperty)DataTree.SelectedNode.Tag).Parent;
                ((WzBinaryProperty)DataTree.SelectedNode.Tag).ParentImage.Changed = true;
                ((WzBinaryProperty)DataTree.SelectedNode.Tag).Remove();
                DataTree.SelectedNode.Tag = prop;
                parent.AddProperty(prop);
                mp3Player.SoundProperty = prop;
            }
        }

        /// <summary>
        /// Saving the sound from WzSoundProperty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_saveSound_Click(object sender, RoutedEventArgs e)
        {
            if (!(DataTree.SelectedNode.Tag is WzBinaryProperty))
                return;
            WzBinaryProperty mp3 = (WzBinaryProperty)DataTree.SelectedNode.Tag;

            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog()
            {
                FileName = mp3.Name,
                Title = "Select where to save the .mp3 file.",
                Filter = "Moving Pictures Experts Group Format 1 Audio Layer 3 (*.mp3)|*.mp3"
            };
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            mp3.SaveToFile(dialog.FileName);
        }

        /// <summary>
        /// Saving the image from WzCanvasProperty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItem_saveImage_Click(object sender, RoutedEventArgs e)
        {
            if (!(DataTree.SelectedNode.Tag is WzCanvasProperty) && !(DataTree.SelectedNode.Tag is WzUOLProperty))
            {
                return;
            }

            System.Drawing.Bitmap wzCanvasPropertyObjLocation = null;
            string fileName = string.Empty;

            if (DataTree.SelectedNode.Tag is WzCanvasProperty)
            {
                WzCanvasProperty canvas = (WzCanvasProperty)DataTree.SelectedNode.Tag;

                wzCanvasPropertyObjLocation = canvas.GetLinkedWzCanvasBitmap();
                fileName = canvas.Name;
            }
            else
            {
                WzObject linkValue = ((WzUOLProperty)DataTree.SelectedNode.Tag).LinkValue;
                if (linkValue is WzCanvasProperty)
                {
                    WzCanvasProperty canvas = (WzCanvasProperty)linkValue;

                    wzCanvasPropertyObjLocation = canvas.GetLinkedWzCanvasBitmap();
                    fileName = canvas.Name;
                }
                else
                    return;
            }
            if (wzCanvasPropertyObjLocation == null)
                return; // oops, we're fucked lulz

            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog()
            {
                FileName = fileName,
                Title = "Select where to save the image...",
                Filter = "Portable Network Grpahics (*.png)|*.png|CompuServe Graphics Interchange Format (*.gif)|*.gif|Bitmap (*.bmp)|*.bmp|Joint Photographic Experts Group Format (*.jpg)|*.jpg|Tagged Image File Format (*.tif)|*.tif"
            };
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            switch (dialog.FilterIndex)
            {
                case 1: //png
                    wzCanvasPropertyObjLocation.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    break;
                case 2: //gif
                    wzCanvasPropertyObjLocation.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Gif);
                    break;
                case 3: //bmp
                    wzCanvasPropertyObjLocation.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                    break;
                case 4: //jpg
                    wzCanvasPropertyObjLocation.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    break;
                case 5: //tiff
                    wzCanvasPropertyObjLocation.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Tiff);
                    break;
            }
        }

        /// <summary>
        /// Export .json, .atlas, as file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItem_ExportFile_Click(object sender, RoutedEventArgs e)
        {
            if (!(DataTree.SelectedNode.Tag is WzStringProperty))
            {
                return;
            }
            WzStringProperty stProperty = DataTree.SelectedNode.Tag as WzStringProperty;

            string fileName = stProperty.Name;
            string value = stProperty.Value;

            string[] fileNameSplit = fileName.Split('.');
            string fileType = fileNameSplit.Length > 1 ? fileNameSplit[fileNameSplit.Length - 1] : "txt";

            System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog()
            {
                FileName = fileName,
                Title = "Select where to save the file...",
                Filter = fileType + " files (*."+ fileType + ")|*."+ fileType + "|All files (*.*)|*.*" 
            }
            ;
            if (saveFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK) 
                return;

            using (System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile())
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(value);
                }
            }
        }
        #endregion

        #region Drag and Drop Image
        private bool bDragEnterActive = false;
        /// <summary>
        /// Scroll viewer drag enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvasPropBox_DragEnter(object sender, DragEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Drag Enter");
            if (!bDragEnterActive)
            {
                bDragEnterActive = true;
            }
        }

        /// <summary>
        ///  Scroll viewer drag leave
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvasPropBox_DragLeave(object sender, DragEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Drag Leave");

            bDragEnterActive = false;
        }
        /// <summary>
        /// Scroll viewer drag drop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvasPropBox_Drop(object sender, DragEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Drag Drop");
            if (bDragEnterActive && DataTree.SelectedNode.Tag is WzCanvasProperty) // only allow button click if its an image property
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files.Length == 0)
                        return;

                    System.Drawing.Bitmap bmp;
                    try
                    {
                        bmp = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(files[0]);
                    }
                    catch (Exception exp)
                    {
                        return;
                    }
                    if (bmp != null)
                        ChangeCanvasPropBoxImage(bmp);

                    //List<UndoRedoAction> actions = new List<UndoRedoAction>(); // Undo action
                }
            }
        }
        #endregion

        #region Copy & Paste
        /// <summary>
        /// Clones a WZ object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private WzObject CloneWzObject(WzObject obj)
        {
            if (obj is WzDirectory)
            {
                Warning.Error(Properties.Resources.MainCopyDirError);
                return null;
            }
            else if (obj is WzImage)
            {
                return ((WzImage)obj).DeepClone();
            }
            else if (obj is WzImageProperty)
            {
                return ((WzImageProperty)obj).DeepClone();
            }
            else
            {
                MapleLib.Helpers.ErrorLogger.Log(MapleLib.Helpers.ErrorLevel.MissingFeature, "The current WZ object type cannot be cloned " + obj.ToString() + " " + obj.FullPath);
                return null;
            }
        }

        /// <summary>
        /// Flag to determine if a copy task is currently active.
        /// </summary>
        private bool
            bPasteTaskActive = false;

        /// <summary>
        /// Copies from the selected Wz object
        /// </summary>
        public void DoCopy()
        {
            if (!Warning.Warn(Properties.Resources.MainConfirmCopy) || bPasteTaskActive)
                return;

            clipboard.Clear();

            foreach (WzNode node in DataTree.SelectedNodes)
            {
                WzObject clone = CloneWzObject((WzObject)((WzNode)node).Tag);
                if (clone != null)
                    clipboard.Add(clone);
            }
        }

        private ReplaceResult replaceBoxResult = ReplaceResult.NoneSelectedYet;

        /// <summary>
        /// Paste to the selected WzObject
        /// </summary>
        public void DoPaste()
        {
            if (!Warning.Warn(Properties.Resources.MainConfirmPaste))
                return;

            bPasteTaskActive = true;
            try
            {
                // Reset replace option
                replaceBoxResult = ReplaceResult.NoneSelectedYet;

                WzNode parent = (WzNode)DataTree.SelectedNode;
                WzObject parentObj = (WzObject)parent.Tag;

                if (parent != null && parent.Tag is WzImage && parent.Nodes.Count == 0)
                {
                    ParseOnDataTreeSelectedItem(parent); // only parse the main node.
                }

                if (parentObj is WzFile)
                    parentObj = ((WzFile)parentObj).WzDirectory;

                bool bNoToAllComplete = false;
                foreach (WzObject obj in clipboard)
                {
                    if (((obj is WzDirectory || obj is WzImage) && parentObj is WzDirectory) || (obj is WzImageProperty && parentObj is IPropertyContainer))
                    {
                        WzObject clone = CloneWzObject(obj);
                        if (clone == null)
                            continue;

                        WzNode node = new WzNode(clone, true);
                        WzNode child = WzNode.GetChildNode(parent, node.Text);
                        if (child != null) // A Child already exist
                        {
                            if (replaceBoxResult == ReplaceResult.NoneSelectedYet)
                            {
                                ReplaceBox.Show(node.Text, out replaceBoxResult);
                            }

                            switch (replaceBoxResult)
                            {
                                case ReplaceResult.No: // Skip just this
                                    replaceBoxResult = ReplaceResult.NoneSelectedYet; // reset after use
                                    break;

                                case ReplaceResult.Yes: // Replace just this
                                    child.DeleteWzNode();
                                    parent.AddNode(node, false);
                                    replaceBoxResult = ReplaceResult.NoneSelectedYet; // reset after use
                                    break;

                                case ReplaceResult.NoToAll:
                                    bNoToAllComplete = true;
                                    break;

                                case ReplaceResult.YesToAll:
                                    child.DeleteWzNode();
                                    parent.AddNode(node, false);
                                    break;
                            }

                            if (bNoToAllComplete)
                                break;
                        }
                        else // not not in this 
                        {
                            parent.AddNode(node, false);
                        }
                    }
                }
            }
            finally
            {
                bPasteTaskActive = false;
            }
        }
        #endregion

        #region UI layout
        /// <summary>
        /// Shows the selected data treeview object to UI
        /// </summary>
        /// <param name="obj"></param>
        private void ShowObjectValue(WzObject obj)
        {
            mp3Player.SoundProperty = null;
            nameBox.Text = obj is WzFile file ? file.Header.Copyright : obj.Name;
            nameBox.ApplyButtonEnabled = false;

            toolStripStatusLabel_additionalInfo.Text = "-"; // Reset additional info to default
            if (isSelectingWzMapFieldLimit) // previously already selected. update again
            {
                isSelectingWzMapFieldLimit = false;
            }

            // Canvas animation
            if (DataTree.SelectedNodes.Count <= 1)
                menuItem_Animate.Visibility = Visibility.Collapsed; // set invisible regardless if none of the nodes are selected.
            else
            {
                bool bIsAllCanvas = true;
                // check if everything selected is WzUOLProperty and WzCanvasProperty
                foreach (WzNode tree in DataTree.SelectedNodes)
                {
                    WzObject wzobj = (WzObject)tree.Tag;
                    if (!(wzobj is WzUOLProperty) && !(wzobj is WzCanvasProperty))
                    {
                        bIsAllCanvas = false;
                        break;
                    }
                }
                menuItem_Animate.Visibility = bIsAllCanvas ? Visibility.Visible : Visibility.Collapsed;
            }

            // Set default layout collapsed state
            mp3Player.Visibility = Visibility.Collapsed;

            // Button collapsed state
            menuItem_changeImage.Visibility = Visibility.Collapsed;
            menuItem_saveImage.Visibility = Visibility.Collapsed;
            menuItem_changeSound.Visibility = Visibility.Collapsed;
            menuItem_saveSound.Visibility = Visibility.Collapsed;
            menuItem_exportFile.Visibility = Visibility.Collapsed;

            // Canvas collapsed state
            canvasPropBox.Visibility = Visibility.Collapsed;

            // Value
            textPropBox.Visibility = Visibility.Collapsed;
            
            // Field limit panel Map.wz/../fieldLimit
            fieldLimitPanelHost.Visibility = Visibility.Collapsed;
            // fieldType panel Map.wz/../fieldType
            fieldTypePanel.Visibility = Visibility.Collapsed;

            // Vector panel
            vectorPanel.Visibility = Visibility.Collapsed;

            // Avalon Text editor
            textEditor.Visibility = Visibility.Collapsed;


            // vars
            bool bIsWzLuaProperty = obj is WzLuaProperty;
            bool bIsWzSoundProperty = obj is WzBinaryProperty;
            bool bIsWzStringProperty = obj is WzStringProperty;
            bool bIsWzIntProperty = obj is WzIntProperty;
            bool bIsWzLongProperty = obj is WzLongProperty;
            bool bIsWzDoubleProperty = obj is WzDoubleProperty;
            bool bIsWzFloatProperty = obj is WzFloatProperty;
            bool bIsWzShortProperty = obj is WzShortProperty;

            bool bAnimateMoreButton = false; // The button to animate when there is more option under button_MoreOption

            // Set layout visibility
            if (obj is WzFile || obj is WzDirectory || obj is WzImage || obj is WzNullProperty || obj is WzSubProperty || obj is WzConvexProperty)
            {
            }
            else if (obj is WzCanvasProperty canvasProp)
            {
                bAnimateMoreButton = true; // flag

                menuItem_changeImage.Visibility = Visibility.Visible;
                menuItem_saveImage.Visibility = Visibility.Visible;

                // Image
                if (canvasProp.HaveInlinkProperty() || canvasProp.HaveOutlinkProperty())
                {
                    System.Drawing.Image img = canvasProp.GetLinkedWzCanvasBitmap();
                    if (img != null)
                        canvasPropBox.Image = ((System.Drawing.Bitmap)img).ToWpfBitmap();
                }
                else
                    canvasPropBox.Image = canvasProp.GetLinkedWzCanvasBitmap().ToWpfBitmap();

                SetImageRenderView(canvasProp);
            }
            else if (obj is WzUOLProperty uolProperty)
            {
                bAnimateMoreButton = true; // flag

                // Image
                WzObject linkValue = uolProperty.LinkValue;
                if (linkValue is WzCanvasProperty canvasUOL)
                {
                    canvasPropBox.Visibility = Visibility.Visible;
                    canvasPropBox.Image = canvasUOL.GetLinkedWzCanvasBitmap().ToWpfBitmap(); // in any event that the WzCanvasProperty is an '_inlink' or '_outlink'
                    menuItem_saveImage.Visibility = Visibility.Visible; // dont show change image, as its a UOL

                    SetImageRenderView(canvasUOL);
                }
                else if (linkValue is WzBinaryProperty binProperty) // Sound, used rarely in wz. i.e Sound.wz/Rune/1/Destroy
                {
                    mp3Player.Visibility = Visibility.Visible;
                    mp3Player.SoundProperty = binProperty;

                    menuItem_changeSound.Visibility = Visibility.Visible;
                    menuItem_saveSound.Visibility = Visibility.Visible;
                }

                // Value
                textPropBox.Visibility = Visibility.Visible;
                textPropBox.ApplyButtonEnabled = false; // reset to disabled mode when changed
                textPropBox.Text = obj.ToString();
            }
            else if (bIsWzSoundProperty)
            {
                bAnimateMoreButton = true; // flag

                mp3Player.Visibility = Visibility.Visible;
                mp3Player.SoundProperty = (WzBinaryProperty)obj;

                menuItem_changeSound.Visibility = Visibility.Visible;
                menuItem_saveSound.Visibility = Visibility.Visible;
            }
            else if (bIsWzLuaProperty)
            {
                textEditor.Visibility = Visibility.Visible;
                textEditor.SetHighlightingDefinitionIndex(2); // javascript

                textEditor.textEditor.Text = obj.ToString();
            }
            else if (bIsWzStringProperty || bIsWzIntProperty || bIsWzLongProperty || bIsWzDoubleProperty || bIsWzFloatProperty || bIsWzShortProperty)
            {
                // If text is a string property, expand the textbox
                if (bIsWzStringProperty)
                {
                    WzStringProperty stringObj = (WzStringProperty)obj;

                    if (stringObj.IsSpineAtlasResources) // spine related resource
                    {
                        bAnimateMoreButton = true;
                        menuItem_exportFile.Visibility = Visibility.Visible;

                        textEditor.Visibility = Visibility.Visible;
                        textEditor.SetHighlightingDefinitionIndex(20); // json
                        textEditor.textEditor.Text = obj.ToString();


                        string path_title = stringObj.Parent?.FullPath ?? "Animate";

                        Thread thread = new Thread(() =>
                        {
                            try
                            {
                                WzSpineAnimationItem item = new WzSpineAnimationItem(stringObj);

                                // Create xna window
                                SpineAnimationWindow Window = new SpineAnimationWindow(item, path_title);
                                Window.Run();
                            }
                            catch (Exception e)
                            {
                                Warning.Error("Error initialising/ rendering spine object. " + e.ToString());
                            }
                        });
                        thread.Start();
                        thread.Join();
                    }
                    else if (stringObj.Name.EndsWith(".json")) // Map001.wz/Back/BM3_3.img/spine/skeleton.json
                    {
                        bAnimateMoreButton = true;
                        menuItem_exportFile.Visibility = Visibility.Visible;

                        textEditor.Visibility = Visibility.Visible;
                        textEditor.SetHighlightingDefinitionIndex(20); // json
                        textEditor.textEditor.Text = obj.ToString();
                    }
                    else
                    {
                        // Value
                        textPropBox.Visibility = Visibility.Visible;
                        textPropBox.Text = obj.ToString();
                        textPropBox.ApplyButtonEnabled = false; // reset to disabled mode when changed

                        if (stringObj.Name == PORTAL_NAME_OBJ_NAME) // Portal type name display - "pn" = portal name 
                        {
                            if (MapleLib.WzLib.WzStructure.Data.Tables.PortalTypeNames.ContainsKey(obj.GetString()))
                            {
                                toolStripStatusLabel_additionalInfo.Text =
                                    string.Format(Properties.Resources.MainAdditionalInfo_PortalType, MapleLib.WzLib.WzStructure.Data.Tables.PortalTypeNames[obj.GetString()]);
                            }
                            else
                            {
                                toolStripStatusLabel_additionalInfo.Text = string.Format(Properties.Resources.MainAdditionalInfo_PortalType, obj.GetString());
                            }
                        }
                        else
                        {
                            textPropBox.AcceptsReturn = true;
                        }
                    }
                }
                else if (bIsWzLongProperty || bIsWzIntProperty || bIsWzShortProperty)
                {
                    textPropBox.Visibility = Visibility.Visible;
                    textPropBox.AcceptsReturn = false;
                    textPropBox.ApplyButtonEnabled = false; // reset to disabled mode when changed

                    // field limit UI
                    if (obj.Name == FIELD_LIMIT_OBJ_NAME) // fieldLimit
                    {
                        isSelectingWzMapFieldLimit = true;

                        ulong value_ = 0;
                        if (bIsWzLongProperty) // use uLong for field limit
                        {
                            value_ = (ulong)((WzLongProperty)obj).GetLong();
                        }
                        else if (bIsWzIntProperty)
                        {
                            value_ = (ulong)((WzIntProperty)obj).GetLong();
                        }
                        else if (bIsWzShortProperty)
                        {
                            value_ = (ulong)((WzShortProperty)obj).GetLong();
                        }

                        fieldLimitPanel1.UpdateFieldLimitCheckboxes(value_);

                        // Set visibility
                        fieldLimitPanelHost.Visibility = Visibility.Visible;
                    } 
                    else 
                    {
                        long value_ = 0; // long for others, in the case of negative value
                        if (bIsWzLongProperty)
                        {
                            value_ = ((WzLongProperty)obj).GetLong();
                        }
                        else if (bIsWzIntProperty)
                        {
                            value_ = ((WzIntProperty)obj).GetLong();
                        }
                        else if (bIsWzShortProperty)
                        {
                            value_ = ((WzShortProperty)obj).GetLong();
                        }
                        textPropBox.Text = value_.ToString();
                    }
                } 
                else if (bIsWzDoubleProperty || bIsWzFloatProperty)
                {
                    textPropBox.Visibility = Visibility.Visible;
                    textPropBox.AcceptsReturn = false;
                    textPropBox.ApplyButtonEnabled = false; // reset to disabled mode when changed

                    if (bIsWzFloatProperty)
                    {
                        textPropBox.Text = ((WzFloatProperty)obj).GetFloat().ToString();
                    } 
                    else if (bIsWzDoubleProperty)
                    {
                        textPropBox.Text = ((WzDoubleProperty)obj).GetDouble().ToString();
                    }
                }
                else
                {
                    textPropBox.AcceptsReturn = false;
                }
            }
            else if (obj is WzVectorProperty property)
            {
                vectorPanel.Visibility = Visibility.Visible;

                vectorPanel.X = property.X.Value;
                vectorPanel.Y = property.Y.Value;
            }
            else
            {
            }

            // Animation button
            if (AnimationBuilder.IsValidAnimationWzObject(obj))
            {
                bAnimateMoreButton = true; // flag

                menuItem_saveImageAnimation.Visibility = Visibility.Visible;
            }
            else
            {
                menuItem_saveImageAnimation.Visibility = Visibility.Collapsed;
            }


            // Storyboard hint
            button_MoreOption.Visibility = bAnimateMoreButton ? Visibility.Visible : Visibility.Collapsed;
            if (bAnimateMoreButton)
            {
                System.Windows.Media.Animation.Storyboard storyboard_moreAnimation = (System.Windows.Media.Animation.Storyboard)(this.FindResource("Storyboard_TreeviewItemSelectedAnimation"));
                storyboard_moreAnimation.Begin();
            }
        }

        /// <summary>
        ///  Sets the ImageRender view on clicked, or via animation tick
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="animationFrame"></param>
        private void SetImageRenderView(WzCanvasProperty canvas)
        {
            // origin
            int? delay = canvas[WzCanvasProperty.AnimationDelayPropertyName]?.GetInt();
            PointF originVector = canvas.GetCanvasOriginPosition();
            PointF headVector = canvas.GetCanvasHeadPosition();
            PointF ltVector = canvas.GetCanvasLtPosition();

            // Set XY point to canvas xaml
            canvasPropBox.ParentWzCanvasProperty = canvas;
            canvasPropBox.Delay = delay ?? 0;
            canvasPropBox.CanvasVectorOrigin = originVector;
            canvasPropBox.CanvasVectorHead = headVector;
            canvasPropBox.CanvasVectorLt = ltVector;

            if (canvasPropBox.Visibility != Visibility.Visible)
                canvasPropBox.Visibility = Visibility.Visible;
        }
        #endregion

        #region Search

        /// <summary>
        /// On search box fade in completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Storyboard_Find_FadeIn_Completed(object sender, EventArgs e)
        {
            findBox.Focus();
        }

        private int searchidx = 0;
        private bool finished = false;
        private bool listSearchResults = false;
        private List<string> searchResultsList = new List<string>();
        private bool searchValues = true;
        private WzNode coloredNode = null;
        private int currentidx = 0;
        private string searchText = "";
        private bool extractImages = false;

        /// <summary>
        /// Close search box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_closeSearch_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Media.Animation.Storyboard sbb = (System.Windows.Media.Animation.Storyboard)(this.FindResource("Storyboard_Find_FadeOut"));
            sbb.Begin();
        }

        private void SearchWzProperties(IPropertyContainer parent)
        {
            foreach (WzImageProperty prop in parent.WzProperties)
            {
                if ((0 <= prop.Name.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase)) || (searchValues && prop is WzStringProperty && (0 <= ((WzStringProperty)prop).Value.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase))))
                {
                    if (listSearchResults)
                        searchResultsList.Add(prop.FullPath.Replace(";", @"\"));
                    else if (currentidx == searchidx)
                    {
                        if (prop.HRTag == null)
                            ((WzNode)prop.ParentImage.HRTag).Reparse();
                        WzNode node = (WzNode)prop.HRTag;
                        //if (node.Style == null) node.Style = new ElementStyle();
                        node.BackColor = System.Drawing.Color.Yellow;
                        coloredNode = node;
                        node.EnsureVisible();
                        //DataTree.Focus();
                        finished = true;
                        searchidx++;
                        return;
                    }
                    else
                        currentidx++;
                }
                if (prop is IPropertyContainer && prop.WzProperties.Count != 0)
                {
                    SearchWzProperties((IPropertyContainer)prop);
                    if (finished)
                        return;
                }
            }
        }

        private void SearchTV(WzNode node)
        {
            foreach (WzNode subnode in node.Nodes)
            {
                if (0 <= subnode.Text.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (listSearchResults)
                        searchResultsList.Add(subnode.FullPath.Replace(";", @"\"));
                    else if (currentidx == searchidx)
                    {
                        //if (subnode.Style == null) subnode.Style = new ElementStyle();
                        subnode.BackColor = System.Drawing.Color.Yellow;
                        coloredNode = subnode;
                        subnode.EnsureVisible();
                        //DataTree.Focus();
                        finished = true;
                        searchidx++;
                        return;
                    }
                    else
                        currentidx++;
                }
                if (subnode.Tag is WzImage)
                {
                    WzImage img = (WzImage)subnode.Tag;
                    if (img.Parsed)
                        SearchWzProperties(img);
                    else if (extractImages)
                    {
                        img.ParseImage();
                        SearchWzProperties(img);
                    }
                    if (finished) return;
                }
                else SearchTV(subnode);
            }
        }

        /// <summary>
        /// Find all
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_allSearch_Click(object sender, RoutedEventArgs e)
        {
            if (coloredNode != null)
            {
                coloredNode.BackColor = System.Drawing.Color.White;
                coloredNode = null;
            }
            if (findBox.Text == "" || DataTree.Nodes.Count == 0)
                return;
            if (DataTree.SelectedNode == null)
                DataTree.SelectedNode = DataTree.Nodes[0];

            finished = false;
            listSearchResults = true;
            searchResultsList.Clear();
            //searchResultsBox.Items.Clear();
            searchValues = Program.ConfigurationManager.UserSettings.SearchStringValues;
            currentidx = 0;
            searchText = findBox.Text;
            extractImages = Program.ConfigurationManager.UserSettings.ParseImagesInSearch;
            foreach (WzNode node in DataTree.SelectedNodes)
            {
                if (node.Tag is WzImageProperty)
                    continue;
                else if (node.Tag is IPropertyContainer)
                    SearchWzProperties((IPropertyContainer)node.Tag);
                else
                    SearchTV(node);
            }

            SearchSelectionForm form = SearchSelectionForm.Show(searchResultsList);
            form.OnSelectionChanged += Form_OnSelectionChanged;

            findBox.Focus();
        }

        /// <summary>
        /// On search selection from SearchSelectionForm list changed
        /// </summary>
        /// <param name="str"></param>
        private void Form_OnSelectionChanged(string str)
        {
            string[] splitPath = str.Split(@"\".ToCharArray());
            WzNode node = null;
            System.Windows.Forms.TreeNodeCollection collection = DataTree.Nodes;
            for (int i = 0; i < splitPath.Length; i++)
            {
                node = GetNodeByName(collection, splitPath[i]);
                if (node != null)
                {
                    if (node.Tag is WzImage && !((WzImage)node.Tag).Parsed && i != splitPath.Length - 1)
                    {
                        ParseOnDataTreeSelectedItem(node, false);
                    }
                    collection = node.Nodes;
                }
            }
            if (node != null)
            {
                DataTree.SelectedNode = node;
                node.EnsureVisible();
                DataTree.RefreshSelectedNodes();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private WzNode GetNodeByName(System.Windows.Forms.TreeNodeCollection collection, string name)
        {
            foreach (WzNode node in collection)
                if (node.Text == name)
                    return node;
            return null;
        }

        /// <summary>
        /// Find next
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_nextSearch_Click(object sender, RoutedEventArgs e)
        {
            if (coloredNode != null)
            {
                coloredNode.BackColor = System.Drawing.Color.White;
                coloredNode = null;
            }
            if (findBox.Text == "" || DataTree.Nodes.Count == 0) return;
            if (DataTree.SelectedNode == null) DataTree.SelectedNode = DataTree.Nodes[0];
            finished = false;
            listSearchResults = false;
            searchResultsList.Clear();
            searchValues = Program.ConfigurationManager.UserSettings.SearchStringValues;
            currentidx = 0;
            searchText = findBox.Text;
            extractImages = Program.ConfigurationManager.UserSettings.ParseImagesInSearch;
            foreach (WzNode node in DataTree.SelectedNodes)
            {
                if (node.Tag is IPropertyContainer)
                    SearchWzProperties((IPropertyContainer)node.Tag);
                else if (node.Tag is WzImageProperty) continue;
                else SearchTV(node);
                if (finished) break;
            }
            if (!finished) { MessageBox.Show(Properties.Resources.MainTreeEnd); searchidx = 0; DataTree.SelectedNode.EnsureVisible(); }
            findBox.Focus();
        }

        private void findBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                button_nextSearch_Click(null, null);
                e.Handled = true;
            }
        }

        private void findBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchidx = 0;
        }
        #endregion

        #region custom
        public void ParseImg()
        {
            Queue<WzNode> Nodes = new Queue<WzNode>();
            foreach (WzNode Node in DataTree.SelectedNodes)
            {
                Nodes.Enqueue(Node);
            }

            StringBuilder sb = new StringBuilder();
            while (Nodes.Count > 0)
            {
                WzNode n = Nodes.Dequeue();
                ParseNode(n, sb);

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
            MessageBox.Show("已完成對選取節點的補圖 - 以下為報表\n\n" + sb);
        }


        public void ParseImg(WzNode node)
        {
            Queue<WzNode> Nodes = new Queue<WzNode>();
            foreach (WzNode Node in node.Nodes)
            {
                Nodes.Enqueue(Node);
            }

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

        public WzFile FindTopNode(String name)
        {
            WzFile ret = null;
            foreach (WzNode data in DataTree.Nodes)
            {
                WzFile file = (WzFile)data.Tag;
                if (file.Name == name + ".wz")
                {
                    ret = file;
                }
            }
            return ret;
        }

        public WzFile FindTopNodeMapEdit(String name)
        {
            WzFile ret = null;
            if (name == "Map002") return FindTopNode("Map002");
            if (HList == null) return getHighWzFile(name + ".wz");
            foreach(WzFile f in HList)
            {
                if(f.Name == name + ".wz")
                {
                    ret = f;
                }
            }

            return ret;
        }



        public void ParseNode(WzNode Node, StringBuilder sb)
        {

            if (Node.Text == "_inlink")
            {
                WzImage TopImage = ((WzImageProperty)Node.Parent.Tag).ParentImage;
                String[] Direction = ((WzStringProperty)Node.Tag).Value.ToString().Split(new char[] { '/' });
                WzImageProperty pointer = TopImage.GetWzImageProperty(Direction[0]);
                if (pointer == null)
                {
                    sb.Append("於: " + ((WzStringProperty)Node.Tag).Value.ToString() + " 中的 : " + Direction[0] + " 出現錯誤\n");
                    return;
                }
                for (int i = 1; i < Direction.Length; i++)
                {
                    pointer = pointer.GetProperty(Direction[i]);
                    if (pointer == null)
                    {
                        sb.Append("於: " + ((WzStringProperty)Node.Tag).Value.ToString() + " 中的 : " + Direction[i] + " 出現錯誤\n");
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

                Boolean found = false;
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
                        found = true;
                        break;
                    }
                }

                if(!found)
                {
                    sb.Append("於: " + ((WzStringProperty)Node.Tag).Value.ToString() + " 中的出現錯誤,對應節點不存在\n");
                }
            }
        }

        public void ParseNode(WzNode Node)
        {
            if (Node.Text == "_inlink")
            {
                WzImage TopImage = ((WzImageProperty)Node.Parent.Tag).ParentImage;
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

                    WzFile TopNode = FindTopNodeMapEdit(Direction[0]);
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

        public List<HashSet<String>> GetMapData()
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
            
            foreach(WzNode node in DataTree.SelectedNodes)
            {
                WzImage img = (WzImage)node.Tag;
                MapData.Add(img.Name.Split(new char[] {'.'})[0]);
                WzImageProperty info = img.GetWzImageProperty("info");
                if(info != null)
                {
                    WzImageProperty bgm = info.GetProperty("bgm");
                    String nsound = ((WzStringProperty)bgm).Value;
                    SoundData.Add(nsound);
                }
                
                foreach(WzImageProperty prop in img.WzProperties) //內容
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
                    }else if (prop.Name.Equals("reactor"))
                    {
                        WzImageProperty reac = prop;
                        foreach(WzImageProperty item in reac.WzProperties)
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

        private List<WzFile> HList;
        private List<WzFile> LList;

        public void setHighWzFile(List<WzFile> x)
        {
            this.HList = x;
        }

        public void setLowWzFile(List<WzFile> x)
        {
            this.LList = x;
        }

        public List<WzFile> getLowFile()
        {
            return LList;
        }

        public WzFile getLowFile(String name)
        {
            foreach(WzFile file in LList)
            {
                if(file.Name == name && (file.Version < 150))
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
                if (file.Name == name && (file.Version >= 150))
                {
                    return file;
                }
            }

            return null;
        }

        public WzNode FindHighNode(String path)
        {
            String[] data = path.Split(new char[] { '/' });
            WzFile file = data[0] == "Map002.wz" ? FindTopNode("Map002") : getHighFile(data[0]); //wz目錄
            if (file == null) return null;
            WzDirectory dic = file.WzDirectory;
            for(int i = 1; i < data.Length-1; i++)
            {
                dic = dic.GetDirectoryByName(data[i]);
                if(dic == null)
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
            WzFile file = data[0] == "Map.wz" ? FindTopNode("Map") : getLowFile(data[0]); //wz目錄
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
            foreach(WzNode n in node.Nodes)
            {
                if (n.Text.Contains("attack"))
                {
                    WzImageProperty prop = (WzImageProperty)n.Tag;
                    WzImageProperty info = prop.GetProperty("info");
                    if(info != null)
                    {
                        WzImageProperty range = info.GetProperty("range");
                        if(range != null && range.GetProperty("sp") != null)
                        {
                            WzVectorProperty sp = (WzVectorProperty)range.GetProperty("sp");
                            WzIntProperty r = (WzIntProperty)range.GetProperty("r");
                            WzVectorProperty rb = new WzVectorProperty("rb", new WzIntProperty("X", Math.Abs(sp.X.Value)), new WzIntProperty("Y", Math.Abs(sp.Y.Value)));
                            WzVectorProperty It = new WzVectorProperty("lt", new WzIntProperty("X", Math.Abs(sp.X.Value)*-1), new WzIntProperty("Y", Math.Abs(sp.Y.Value)*-1));
                            ((WzSubProperty)range).AddProperty(rb);
                            ((WzSubProperty)range).AddProperty(It);
                            ((WzSubProperty)range).RemoveProperty(sp);
                            if(r != null) ((WzSubProperty)range).RemoveProperty(r);
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
            foreach(WzImageProperty pp in img.WzProperties)
            {
                WzNode node = new WzNode(pp);
                que.Enqueue(node);
            }
            
            while(que.Count > 0)
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
            WzDirectory file = FindTopNode("Map").WzDirectory;
            if (file == null) return new Tuple<HashSet<String>, HashSet<String>>(list,main);

            WzDirectory dic = file.GetDirectoryByName("Map");
            foreach(WzDirectory p in dic.WzDirectories)
            {
                foreach(WzImage img in p.WzImages)
                {
                    foreach(WzImageProperty ii in img.WzProperties)
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
            foreach(WzImageProperty prop in img.WzProperties.ToArray())
            {
                if(prop.Name.Contains("condition"))
                {
                    prop.Remove();
                }

                if(prop.Name == "info")
                {
                    foreach(WzImageProperty pt in prop.WzProperties.ToArray())
                    {
                        if(pt.Name == "link")
                        {
                            String id;
                            if(pt is WzStringProperty)
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
                                if(linkadd != null)
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


        public void AddMap()
        {
            List<HashSet<String>> data = GetMapData();
            StringBuilder sb = new StringBuilder();
            StringBuilder error = new StringBuilder();
            sb.Append("-----------地圖同步\n\n");
            sb.Append("[已讀取低版本WZ檔案(Map.wz以外)]\n");
            foreach(WzFile fx in LList)
            {
                sb.Append(fx.Name + ",");
            }
            sb.Append("\n\n");
            sb.Append("[已讀取高版本WZ檔案(Map002.wz以外)]\n");
            foreach (WzFile fx in HList)
            {
                sb.Append(fx.Name + ",");
            }
            sb.Append("\n\n\n");
            error.Append("-----------錯誤訊息\n\n");
            error.Append("報錯訊息多是因檔案讀取不完全,請將高版本對應WZ補齊\n");

            //dealing map
            String[] obj = { "Map.wz", "Map2.wz" };
            String[] back = { "Map001.wz", "Map2.wz" };
            String tile = "Map.wz";
            String[] Mob = { "Mob.wz", "Mob2.wz", "Mob001.wz" };
            String[] Sound = { "Sound.wz", "Sound001.wz", "Sound2.wz" };

            HashSet<String> ObjIndex = data[0];
            foreach(String s in ObjIndex)
            {
                WzNode n = null;
                Boolean found = false;
                foreach(String head in obj)
                {
                    n = FindHighNode(head + "/Obj/" + s);
                    if (n != null)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) error.Append("[錯誤訊息] : 並未找到物件 " + "Obj/" + s + "\n");
                if (n == null) continue;
                WzDirectory dic = FindLowNode("Map.wz/Obj/" + s);
                if(dic == null)
                {
                    error.Append("[錯誤訊息] : 並未找到原始目錄 " + "Obj/" + s + "\n");
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
                sb.Append("已成功新增物件 " + img.Name + "\n");
            }

           

            HashSet<String> BackIndex = data[1];
            
            foreach (String s in BackIndex)
            {
                WzNode n = null;
                Boolean found = false;
                foreach (String head in back)
                {
                    n = FindHighNode(head + "/Back/" + s);
                    if(n != null)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found && s.Length > 0) error.Append("[錯誤訊息] : 並未找到物件 " + "Back/" + s + "\n");
                if (n == null) continue;
                WzDirectory dic = FindLowNode("Map.wz/Back/" + s);
                if (dic == null)
                {
                    error.Append("[錯誤訊息] : 並未找到原始目錄 " + "Back/" + s + "\n");
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
                sb.Append("已成功新增背景 " + img.Name + "\n");
            }

            HashSet<String> TileIndex = data[2];
            foreach (String s in TileIndex)
            {
                WzNode n = FindHighNode(tile + "/Tile/" + s);
                if (n == null) error.Append("[錯誤訊息] : 並未找到磚塊 " + "Tile/" + s + "\n");
                if (n == null) continue;
                WzDirectory dic = FindLowNode("Map.wz/Tile/" + s);
                if (dic == null)
                {
                    error.Append("[錯誤訊息] : 並未找到原始目錄 " + "Tile/" + s + "\n");
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
                sb.Append("已成功新增磚塊 " + img.Name + "\n");
            }

            HashSet<String> NpcIndex = data[3];
            foreach(String s in NpcIndex)
            {
                WzNode n = FindHighNode("Npc.wz/" + s);
                if (n == null) error.Append("[錯誤訊息] : 並未找到NPC " + s + "\n");
                if (n == null) continue;
                WzDirectory dic = FindLowNode("Npc.wz/" + s);
                if (dic == null)
                {
                    error.Append("[錯誤訊息] : 並未找到原始目錄 " + "Npc.wz/" + s + "\n");
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
                    sb.Append("已成功新增NPC " + img.Name + "\n");
                }
            }

            HashSet<String> MobData = data[4];
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
                if (!found) error.Append("[錯誤訊息] : 並未找到怪物 "  + s + "\n");
                if (n == null) continue;
                WzDirectory dic = FindLowNode("Mob.wz/" + s);
                if (dic == null)
                {
                    error.Append("[錯誤訊息] : 並未找到原始目錄 " + "Mob.wz/" + s + "\n");
                    continue;
                }
                WzImage img = (WzImage)n.Tag;
                img.ParseImage();
                WzNode x = new WzNode(img);
                ParseImg(x);
                if(LList[0].Version < 145)
                {
                    modifyLowMob(x);
                }
                addMobRevive(x, data[6], data[8]); //加資料到stringdata
                if (dic.GetImageByName(img.Name) == null)
                {
                    dic.AddImage(img.DeepClone());
                    dic.GetImageByName(img.Name).Changed = true;
                    sb.Append("已成功新增怪物 " + img.Name + "\n");
                }
            }

            HashSet<String> MapData = data[5];
            Tuple<HashSet<String>, HashSet<String>> tu = getLowMapInfo();
            HashSet<String> list = tu.Item1;
            HashSet<String> main = tu.Item2;
            foreach (String s in MapData)
            {
                String index = (int.Parse(s) / 100000000).ToString();
                WzNode n = FindHighNode("Map002.wz/Map/Map" + index + "/" + s);
                if (n == null) error.Append("[錯誤訊息] : 並未找到地圖 " + s + "\n");
                if (n == null) continue;
                WzImage img = (WzImage)n.Tag;
                img.ParseImage();
                foreach(WzImageProperty ii in img.WzProperties.ToArray())
                {
                    if (!main.Contains(ii.Name))
                    {
                        ii.Remove();
                    }
                    if(ii.Name == "info")
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
                WzDirectory dic = FindTopNode("Map").WzDirectory;
                WzDirectory dic2 = dic.GetDirectoryByName("Map");
                if(dic2 == null)
                {
                    error.Append("[錯誤訊息] : 並未找到原始目錄 " + "Map.wz/Map\n");
                    continue;
                }
                WzDirectory dic3 = dic2.GetDirectoryByName("Map" + index);
                if(dic3 == null)
                {
                    WzDirectory newDic = new WzDirectory("Map" + index, FindTopNode("Map"));
                    newDic.AddImage(img.DeepClone());
                    dic2.AddDirectory(newDic);
                }
                else
                {
                    dic3.AddImage(img.DeepClone());
                }   
            }
            sb.Append("已成功新增地圖(含MARK)\n");
            //以下處理String
            WzFile HString = getHighFile("String.wz");
            WzFile LString = getLowFile("String.wz");
            HashSet<String> StringData = data[6];
            if(HString != null && LString != null)
            {
                foreach (String s in StringData)
                {
                    String[] x = s.Split(new char[] { '/' });
                    WzImage Himg = HString.WzDirectory.GetImageByName(x[0]);
                    WzImage Limg = LString.WzDirectory.GetImageByName(x[0]);
                    if(Himg != null && Limg != null)
                    {
                        WzImageProperty prop = Himg.GetWzImageProperty(x[1]);
                        if (prop != null && Limg.GetWzImageProperty(x[1]) == null)
                        {
                            Limg.AddProperty(prop.DeepClone());
                            Limg.Changed = true;
                        }
                    }
                    else
                    {
                        error.Append("[錯誤訊息] : 並未找到STRING " + s + "\n");
                    }
                }
            }
            

            mark();

            //以下處理Reactor
            HashSet<String> ReactorData = data[7];
            foreach(String s in ReactorData)
            {
                WzNode n = FindHighNode("Reactor.wz/" + s);
                if (n == null) error.Append("[錯誤訊息] : 並未找到反應堆 " + s + "\n");
                if (n == null) continue;
                WzDirectory dic = FindLowNode("Reactor.wz/" + s);
                if (dic == null)
                {
                    error.Append("[錯誤訊息] : 並未找到原始目錄 " + "Reactor.wz/" + s + "\n");
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
                    sb.Append("已成功新增Reactor " + img.Name + "\n");
                }
            }


            HashSet<String> SoundData = data[8];
            foreach(String s in SoundData)
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

                if(!found) error.Append("[錯誤訊息] : 並未找到音效 " + s + "\n");
            }
            MessageBox.Show(sb.ToString() + "\n" + error.ToString(), "地圖同步程序");
        }

       

        public void mark()
        {
            WzFile HString = getHighFile("String.wz");
            WzFile LString = getLowFile("String.wz");
            if(HString != null && LString != null)
            {
                WzImage mapStringImg = HString.WzDirectory.GetImageByName("Map.img");
                WzImage toolTipImg = HString.WzDirectory.GetImageByName("ToolTipHelp.img");
                if(mapStringImg != null)
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

                if(toolTipImg != null)
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
            WzFile Lmark = FindTopNode("Map");
            if(Hmark != null && Lmark != null)
            {
                WzImage HImg = Hmark.WzDirectory.GetImageByName("MapHelper.img");
                WzImage LImg = Lmark.WzDirectory.GetImageByName("MapHelper.img");
                if(HImg != null && LImg != null)
                {
                    WzImageProperty HMMark = HImg.GetWzImageProperty("mark");
                    if(HMMark != null)
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


        public void ReadNode()
        {
            List<HashSet<String>> read = new List<HashSet<String>>();
            Queue<Tuple<WzNode, int>> que = new Queue<Tuple<WzNode, int>>();

            que.Enqueue(new Tuple<WzNode,int>((WzNode)DataTree.SelectedNode, 0));

   
            while(que.Count > 0)
            {
                Tuple<WzNode, int> current = que.Dequeue();
                int layer = current.Item2;
                WzNode node = current.Item1;

                if(read.Count == layer)
                {
                    read.Add(new HashSet<String>());
                }

                read[layer].Add(node.Text);
                
                if((WzObject)node.Tag is WzImage)
                {
                    WzImage img = (WzImage)node.Tag;
                    img.ParseImage();
                    foreach(WzImageProperty prop in img.WzProperties)
                    {
                        WzNode n = new WzNode(prop);
                        que.Enqueue(new Tuple<WzNode,int>(n, layer+1));
                    }
                }
                else
                {
                    foreach(WzNode n in node.Nodes)
                    {
                        que.Enqueue(new Tuple<WzNode, int>(n, layer + 1));
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            int a = 0;
            foreach(HashSet<String> set in read)
            {
                sb.Append(a);
                foreach(String n in set)
                {
                    int num = 0;
                    /*if (!int.TryParse(n, out num) && !n.Contains(".img"))*/ sb.Append(n + ",");
                }
                sb.Append("\n\n");
                a++;
            }

            MessageBox.Show(sb.ToString());
        }

        private WzFile getLowWzFile(String name)
        {
            foreach(WzNode n in DataTree.Nodes)
            {
                WzFile file = (WzFile)n.Tag;
                if(file.Name == name && file.Version < 150)
                {
                    return file;
                }
            }
            return null;
        }

        private WzFile getHighWzFile(String name)
        {
            foreach (WzNode n in DataTree.Nodes)
            {
                WzFile file = (WzFile)n.Tag;
                if (file.Name == name && file.Version >= 150)
                {
                    return file;
                }
            }
            return null;
        }

        private WzImageProperty getSkillPath(String path)
        {
            WzFile LS = getLowWzFile("Skill.wz");
            String[] p = path.Split(new char[] { '/' });
            WzImage img = LS.WzDirectory.GetImageByName(p[0]);
            if (img == null) return null;

            WzImageProperty s = img.GetWzImageProperty(p[1]);
            if (s == null) return null;

            WzImageProperty skill = s.GetProperty(p[2]);
            if (skill == null) return null;

            if (skill.GetProperty("effect") == null) return null;
            foreach(WzImageProperty prop in skill.WzProperties.ToArray())
            {
                if (prop.Name.Contains("effect")) prop.Remove();
            }

            return skill;
        }

        public void changeSkillAnimation()
        {
            WzFile LS = getLowWzFile("Skill.wz");
            WzFile HS = getHighWzFile("Skill.wz");

            if(LS == null)
            {
                MessageBox.Show("請加載低版本Skill.wz");
            }

            foreach(WzNode n in DataTree.SelectedNodes)
            {
                WzObject obj = (WzObject)n.Tag;
                if (!(obj is WzImage)) continue;

                WzImage img = (WzImage)obj;
                WzImageProperty s = img.GetWzImageProperty("skill");
                foreach(WzImageProperty prop in s.WzProperties)
                {
                    WzImageProperty effect = prop.GetProperty("effect");
                    if(effect == null) continue;
                    WzImageProperty LP = getSkillPath(img.Name + "/skill/" + prop.Name);
                    if (LP != null)
                    {
                        foreach (WzImageProperty p in prop.WzProperties)
                        {
                            if (p.Name.Contains("effect"))
                            {
                                WzNode nx = new WzNode(p);
                                ParseImg(nx);
                                ((WzSubProperty)LP).AddProperty(p.DeepClone());

                            }
                        }
                        LP.ParentImage.Changed = true;

                        if (LP.ParentImage.Name.Split(new char[] { '.' })[0].Length <= 3)
                        {
                            foreach (WzImageProperty pic in LP.WzProperties)
                            {
                                if (pic.Name.Contains("effect"))
                                {
                                    foreach (WzImageProperty canvas in pic.WzProperties)
                                    {
                                        if (canvas is WzCanvasProperty)
                                        {
                                            ((WzCanvasProperty)canvas).PngProperty.ListWzUsed = true;
                                        }
                                    }
                                }
                            }
                        }

                        LP.ParentImage.Changed = true;
                    }
                    
                }
            }
        }

        private WzImageProperty getSillById(String id)
        {
            WzDirectory dic = FindTopNode("Skill").WzDirectory;
            String imgName = "";
            if(id.Length == 8)
            {
                imgName = id.Substring(0, 4) + ".img";
            }
            else if(id.Length == 7)
            {
                imgName = id.Substring(0, 3) + ".img";
            }

            WzImage img = dic.GetImageByName(imgName);
            if (img == null) return null;

            WzImageProperty next = img.GetWzImageProperty("skill");
            if (next == null) return null;

            WzImageProperty ret = next.GetProperty(id);
            return ret;
        }

        public void editSkill(StreamReader sr)
        {
            String line = sr.ReadLine();
            Boolean multiMob = false;
            while(line != null)
            {
                if (line == "允許攻擊多怪") multiMob = true;
                String[] data = line.Split(new char[] { '/' });
                String id = data[0];
                WzImageProperty skill = getSillById(id);
                int[] list = new int[8];

                if (skill != null)
                {
                    WzImageProperty level = skill.GetProperty("level");

                    for (int i = 1; i < data.Length; i++)
                    {
                        String para = data[i];

                        if (para.Contains("傷害"))
                        {
                            String modify = para.Split(new char[] { '害' })[1];
                            int add = 0;
                            int percent = 0;
                            if (modify.Contains("+"))
                            {
                                if (modify.Contains("%"))
                                {
                                    percent = int.Parse(modify.Substring(1, modify.Length - 2));
                                }
                                else
                                {
                                    add = int.Parse(modify.Substring(1, modify.Length-1));
                                }

                            }
                            else if (modify.Contains("-"))
                            {
                                if (modify.Contains("%"))
                                {
                                    percent = int.Parse(modify.Substring(1, modify.Length - 2)) * -1;
                                }
                                else
                                {
                                    add = int.Parse(modify.Substring(1, modify.Length-1)) * -1;
                                }
                            }
                            list[0] = add;
                            list[1] = percent;
                        }

                        if (para.Contains("段數"))
                        {
                            String modify = para.Split(new char[] { '數' })[1];
                            int add = 0;
                            if (modify.Contains("+"))
                            {
                                add = int.Parse(modify.Substring(1, modify.Length - 1));

                            }
                            else if (modify.Contains("-"))
                            {
                                add = int.Parse(modify.Substring(1, modify.Length - 1)) * -1;
                            }


                            list[2] = add;
                        }

                        if (para.Contains("CD"))
                        {
                            String modify = para.Split(new char[] { 'D' })[1];
                            int add = 0;
                            if (modify.Contains("+"))
                            {
                                add = int.Parse(modify.Substring(1, modify.Length - 1));

                            }
                            else if (modify.Contains("-"))
                            {
                                add = int.Parse(modify.Substring(1, modify.Length - 1)) * -1;
                            }else if(modify == "0")
                            {
                                add = 999;
                            }

                            list[3] = add;
                        }

                        if (para.Contains("打怪"))
                        {
                            String modify = para.Split(new char[] { '怪' })[1];
                            int add = 0;
                            if (modify.Contains("+"))
                            {
                                add = int.Parse(modify.Substring(1, modify.Length - 1));

                            }
                            else if (modify.Contains("-"))
                            {
                                add = int.Parse(modify.Substring(1, modify.Length - 1)) * -1;
                            }
                            list[4] = add;
                        }

                        if (para.Contains("時間"))
                        {
                            String modify = para.Split(new char[] { '間' })[1];
                            int add = 0;
                            if (modify.Contains("+"))
                            {
                                add = int.Parse(modify.Substring(1, modify.Length - 1));

                            }
                            else if (modify.Contains("-"))
                            {
                                add = int.Parse(modify.Substring(1, modify.Length - 1)) * -1;
                            }
                            list[5] = add;
                        }

                        if (para.Contains("範圍"))
                        {
                            String modify = para.Split(new char[] { '圍' })[1];
                            int addX = 0;
                            int addY = 0;
                            if (modify.Contains("X"))
                            {
                                if (modify.Contains("+")) 
                                {
                                    addX = int.Parse(modify.Substring(2, modify.Length - 2));
                                }else if (modify.Contains("-"))
                                {
                                    addX = int.Parse(modify.Substring(2, modify.Length - 2))*-1;
                                }
                                

                            }
                            else if (modify.Contains("Y"))
                            {
                                if (modify.Contains("+"))
                                {
                                    addY = int.Parse(modify.Substring(2, modify.Length - 2));
                                }
                                else if (modify.Contains("-"))
                                {
                                    addY = int.Parse(modify.Substring(2, modify.Length - 2)) * -1;
                                }
                            }
                            else
                            {
                                if (modify.Contains("+"))
                                {
                                    addY = int.Parse(modify.Substring(1, modify.Length - 2));
                                    addX = int.Parse(modify.Substring(1, modify.Length - 2));
                                }
                                else if (modify.Contains("-"))
                                {
                                    addY = int.Parse(modify.Substring(1, modify.Length - 2)) * -1;
                                    addX = int.Parse(modify.Substring(1, modify.Length - 2)) * -1;
                                }
                            }
                            list[6] = addX;
                            list[7] = addY;
                        }
                    }

                   
                    if (level == null) continue;
                    foreach (WzImageProperty p in level.WzProperties.ToArray())
                    {
                        //傷害,傷害%,段數,cd,打怪數,時間,範圍X,範圍Y,
                        //處理傷害
                        WzImageProperty damage = p.GetProperty("damage");

                        if (damage != null)
                        {
                            if (list[0] != 0)
                            {
                                if (damage is WzIntProperty)
                                {
                                    int value = ((WzIntProperty)damage).Value;
                                    ((WzIntProperty)damage).Value = value + list[0];
                                }
                                else if (damage is WzStringProperty)
                                {
                                    int value = int.Parse(((WzStringProperty)damage).Value);
                                    ((WzStringProperty)damage).Value = (value + list[0]).ToString();
                                }
                            }

                            if (list[1] != 0)
                            {
                                if (damage is WzIntProperty)
                                {
                                    int value = ((WzIntProperty)damage).Value;
                                    ((WzIntProperty)damage).Value = value * (100 + list[1]) / 100;
                                }
                                else if (damage is WzStringProperty)
                                {
                                    int value = int.Parse(((WzStringProperty)damage).Value);
                                    ((WzStringProperty)damage).Value = (value * (100 + list[1]) / 100).ToString();
                                }
                            }
                            
                        }

                        //處理段數
                        WzImageProperty bulletC = p.GetProperty("bulletCount");
                        WzImageProperty attackC = p.GetProperty("attackCount");
                        if(bulletC != null && list[2] != 0)
                        {
                            if (bulletC is WzIntProperty)
                            {
                                int value = ((WzIntProperty)bulletC).Value;
                                ((WzIntProperty)bulletC).Value = value + list[2];
                            }
                            else if (bulletC is WzStringProperty)
                            {
                                int value = int.Parse(((WzStringProperty)bulletC).Value);
                                ((WzStringProperty)bulletC).Value = (value + list[2]).ToString();
                            }
                        }

                        if(attackC != null && list[2] != 0)
                        {
                            if (attackC is WzIntProperty)
                            {
                                int value = ((WzIntProperty)attackC).Value;
                                ((WzIntProperty)attackC).Value = value + list[2];
                            }
                            else if (attackC is WzStringProperty)
                            {
                                int value = int.Parse(((WzStringProperty)attackC).Value);
                                ((WzStringProperty)attackC).Value = (value + list[2]).ToString();
                            }
                        }
                        else if(list[2] != 0)
                        {
                            WzIntProperty newAttackC = null;

                            if (skill.GetProperty("ball") == null)
                            {
                                newAttackC = new WzIntProperty("attackCount", list[2]);
                            }
                            else
                            {
                                newAttackC = new WzIntProperty("bulletCount", list[2]);
                            }
                            ((WzSubProperty)p).AddProperty(newAttackC);
                        }

                        //處理CD
                        WzImageProperty cooldown = p.GetProperty("cooltime");
                        if (cooldown != null && list[3] != 0)
                        {
                            if (cooldown is WzIntProperty)
                            {
                                int value = ((WzIntProperty)cooldown).Value;
                                if(list[3] == 999)
                                {
                                    ((WzIntProperty)cooldown).Value = 0;
                                }
                                else
                                {
                                    ((WzIntProperty)cooldown).Value = value + list[3];
                                }
                                
                            }
                            else if (cooldown is WzStringProperty)
                            {
                                int value = int.Parse(((WzStringProperty)cooldown).Value);
                                if(list[3] == 999)
                                {
                                    ((WzStringProperty)cooldown).Value = "0";
                                }
                                else
                                {
                                    ((WzStringProperty)cooldown).Value = (value + list[3]).ToString();
                                }
                               
                            }
                        }

                        //處理打怪數
                        WzImageProperty mobC = p.GetProperty("mobCount");
                        if(mobC != null && list[4] != 0)
                        {
                            if (mobC is WzIntProperty)
                            {
                                int value = ((WzIntProperty)mobC).Value;
                                ((WzIntProperty)mobC).Value = value + list[4];
                            }
                            else if (mobC is WzStringProperty)
                            {
                                int value = int.Parse(((WzStringProperty)mobC).Value);
                                ((WzStringProperty)mobC).Value = (value + list[4]).ToString();
                            }
                        }

                        if(mobC == null && multiMob && list[4] != 0)
                        {
                            WzIntProperty newMobC = new WzIntProperty("mobCount", list[4]);
                            WzVectorProperty rb = new WzVectorProperty("rb", new WzIntProperty("X", 150), new WzIntProperty("Y", 150));
                            WzVectorProperty lt = new WzVectorProperty("lt", new WzIntProperty("X", -150), new WzIntProperty("Y", -150));
                            ((WzSubProperty)p).AddProperty(newMobC);
                            ((WzSubProperty)p).AddProperty(rb);
                            ((WzSubProperty)p).AddProperty(lt);
                        }

                        //處理時間
                        WzImageProperty time = p.GetProperty("time");
                        if(time != null && list[5] != 0)
                        {
                            if (time is WzIntProperty)
                            {
                                int value = ((WzIntProperty)time).Value;
                                ((WzIntProperty)time).Value = value + list[5];
                            }
                            else if (time is WzStringProperty)
                            {
                                int value = int.Parse(((WzStringProperty)time).Value);
                                ((WzStringProperty)time).Value = (value + list[5]).ToString();
                            }
                        }
                        
                        //處理範圍
                        WzImageProperty range = p.GetProperty("range");
                        WzImageProperty rbR = p.GetProperty("rb");
                        WzImageProperty ltR = p.GetProperty("lt");
                        if(range != null && list[6] != 0)
                        {
                            if (range is WzIntProperty)
                            {
                                int value = ((WzIntProperty)range).Value;
                                ((WzIntProperty)range).Value = value + list[6];
                            }
                            else if (range is WzStringProperty)
                            {
                                int value = int.Parse(((WzStringProperty)range).Value);
                                ((WzStringProperty)range).Value = (value + list[6]).ToString();
                            }
                        }

                        if(rbR != null && ltR != null && (list[6] != 0 || list[7] != 0))
                        {
                            
                            int xr = ((WzVectorProperty)rbR).X.Value * (100 + list[6]) / 100;
                            int yr = ((WzVectorProperty)rbR).Y.Value * (100 + list[7]) / 100;
                            int xl = ((WzVectorProperty)ltR).X.Value * (100 + list[6]) / 100;
                            int yl = ((WzVectorProperty)ltR).Y.Value * (100 + list[7]) / 100;

                            ((WzVectorProperty)rbR).X.Value = xr;
                            ((WzVectorProperty)rbR).Y.Value = yr;
                            ((WzVectorProperty)ltR).X.Value = xl;
                            ((WzVectorProperty)ltR).Y.Value = yl;
                        }

                    }

                    skill.ParentImage.Changed = true;
                }
                line = sr.ReadLine();
            }

            MessageBox.Show("修改完畢", "技能平衡");
        }

        private WzFile getTopNode(String name)
        {
            foreach(WzNode n in DataTree.Nodes)
            {
                if (n.Text == name) return (WzFile)n.Tag;
            }
            return null;
        }


        private int GetIntValue(WzImageProperty p)
        {
            if (p == null) return -1;

            int ret = 0;
            if (p is WzIntProperty) ret = ((WzIntProperty)p).Value;
            else if (p is WzStringProperty)
            {
                String s = ((WzStringProperty)p).Value;
                if (int.TryParse(s, out ret))
                {
                    ret = int.Parse(s);
                }
            }
            return ret;
        }


        private Boolean IsValidCash(Dictionary<String, int> data, List<Tuple<String, int>> check)
        {
            foreach(Tuple<String, int> pair in check)
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

        private SortedSet<int> GetCashItem(String name, List<Tuple<String, int>> check)
        {
            SortedSet<int> ret = new SortedSet<int>();
            foreach (WzFile file in cashFile)
            {
                WzDirectory dic = file.WzDirectory.GetDirectoryByName(name);

                if (dic != null)
                {
                    foreach (WzImage img in dic.WzImages)
                    {
                        WzImageProperty info = img.GetWzImageProperty("info");
                        Dictionary<String, int> data = new Dictionary<String, int>();
                        foreach (WzImageProperty p in info.WzProperties)
                        {
                            data.Add(p.Name, GetIntValue(p));
                        }
                        if (IsValidCash(data, check))
                        {
                            ret.Add(int.Parse(img.Name.Split(new char[] { '.' })[0]));
                        }
                    }
                }
            }
            return ret;
        }

        private WzImage InitCommodity()
        {
            WzFile file = getTopNode("Etc.wz");
            WzImage commodity = file.WzDirectory.GetImageByName("Commodity.img");
            if(commodity != null) commodity.Remove();
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

        private List<WzFile> cashFile = new List<WzFile>();


        public void CreateCommodity()

        {
            foreach(WzNode node in DataTree.Nodes)
            {
                if (node.Text.Contains("Character")) cashFile.Add((WzFile)node.Tag);
            }

            String[] type = { "Cap", "Accessory", "Accessory", "LongCoat", "Coat", "Pants", "Shoes", "Gloves", "Weapon", "Ring", "", "Cape"};
            List<SortedSet<int>> data = new List<SortedSet<int>>();

            List<Tuple<String, int>> check = new List<Tuple<String, int>>();
            /**add parameter*/
            check.Add(new Tuple<String, int>("cash", 1));
            check.Add(new Tuple<String, int>("incPAD", 0));
            check.Add(new Tuple<String, int>("incMAD", 0));

            foreach (String s in type)
            {
                data.Add(GetCashItem(s,check));
            }

            WzImage img = InitCommodity();
            int index = 0;
            int serial = 20000000;
            for (int i = 0; i < data.Count; i++)
            {
                int num = 0;
                foreach(int item in data[i])
                {
                    int sn = serial + i * 100000 + num;
                    AddNewCommodity(img, index, 1, item, sn);
                    num++;
                    index++;
                }
            }
            img.Changed = true;
        }


        #endregion
    }
}
