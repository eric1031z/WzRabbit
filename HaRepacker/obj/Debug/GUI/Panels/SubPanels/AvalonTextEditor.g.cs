#pragma checksum "..\..\..\..\..\GUI\Panels\SubPanels\AvalonTextEditor.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "E82393EB1F04D74BDD328CE63A728732CBE13386CC164936B1708373FCBD9C3A"
//------------------------------------------------------------------------------
// <auto-generated>
//     這段程式碼是由工具產生的。
//     執行階段版本:4.0.30319.42000
//
//     對這個檔案所做的變更可能會造成錯誤的行為，而且如果重新產生程式碼，
//     變更將會遺失。
// </auto-generated>
//------------------------------------------------------------------------------

using HaRepacker.GUI.Panels.SubPanels;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace HaRepacker.GUI.Panels.SubPanels {
    
    
    /// <summary>
    /// AvalonTextEditor
    /// </summary>
    public partial class AvalonTextEditor : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 22 "..\..\..\..\..\GUI\Panels\SubPanels\AvalonTextEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox comboBox_SyntaxHighlightingType;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\..\..\..\GUI\Panels\SubPanels\AvalonTextEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button button_saveApply;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\..\..\GUI\Panels\SubPanels\AvalonTextEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal ICSharpCode.AvalonEdit.TextEditor textEditor;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/HaRepackerResurrected;component/gui/panels/subpanels/avalontexteditor.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\GUI\Panels\SubPanels\AvalonTextEditor.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.comboBox_SyntaxHighlightingType = ((System.Windows.Controls.ComboBox)(target));
            
            #line 29 "..\..\..\..\..\GUI\Panels\SubPanels\AvalonTextEditor.xaml"
            this.comboBox_SyntaxHighlightingType.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.comboBox_SyntaxHighlightingType_SelectionChanged_1);
            
            #line default
            #line hidden
            return;
            case 2:
            this.button_saveApply = ((System.Windows.Controls.Button)(target));
            
            #line 39 "..\..\..\..\..\GUI\Panels\SubPanels\AvalonTextEditor.xaml"
            this.button_saveApply.Click += new System.Windows.RoutedEventHandler(this.button_saveApply_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.textEditor = ((ICSharpCode.AvalonEdit.TextEditor)(target));
            
            #line 47 "..\..\..\..\..\GUI\Panels\SubPanels\AvalonTextEditor.xaml"
            this.textEditor.TextChanged += new System.EventHandler(this.textEditor_TextChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

