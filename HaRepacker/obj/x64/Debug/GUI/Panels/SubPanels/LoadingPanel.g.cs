#pragma checksum "..\..\..\..\..\..\GUI\Panels\SubPanels\LoadingPanel.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "82983C688066474F19F46710DCDC34CA5E5BBD93BE97BB366041F5619EF9542A"
//------------------------------------------------------------------------------
// <auto-generated>
//     這段程式碼是由工具產生的。
//     執行階段版本:4.0.30319.42000
//
//     對這個檔案所做的變更可能會造成錯誤的行為，而且如果重新產生程式碼，
//     變更將會遺失。
// </auto-generated>
//------------------------------------------------------------------------------

using HaRepacker.Converter;
using HaRepacker.GUI.Panels.SubPanels;
using HaRepacker.Properties;
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
using WpfAnimatedGif;


namespace HaRepacker.GUI.Panels.SubPanels {
    
    
    /// <summary>
    /// LoadingPanel
    /// </summary>
    public partial class LoadingPanel : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 28 "..\..\..\..\..\..\GUI\Panels\SubPanels\LoadingPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image imageLoadingGif;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\..\..\..\GUI\Panels\SubPanels\LoadingPanel.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel stackPanel_wzIvBruteforceStat;
        
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
            System.Uri resourceLocater = new System.Uri("/HaRepackerResurrected;component/gui/panels/subpanels/loadingpanel.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\..\GUI\Panels\SubPanels\LoadingPanel.xaml"
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
            this.imageLoadingGif = ((System.Windows.Controls.Image)(target));
            
            #line 30 "..\..\..\..\..\..\GUI\Panels\SubPanels\LoadingPanel.xaml"
            this.imageLoadingGif.AddHandler(WpfAnimatedGif.ImageBehavior.AnimationLoadedEvent, new System.Windows.RoutedEventHandler(this.ImageLoadingGif_AnimationLoaded));
            
            #line default
            #line hidden
            return;
            case 2:
            this.stackPanel_wzIvBruteforceStat = ((System.Windows.Controls.StackPanel)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

