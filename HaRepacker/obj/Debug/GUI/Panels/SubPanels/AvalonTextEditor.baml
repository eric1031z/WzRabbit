   M S B A M L   `   `   `  ����P  LHaRepackerResurrected, Version=4.2.4.0, Culture=neutral, PublicKeyToken=nullQ-clr-namespace:HaRepacker.GUI.Panels.SubPanelsHaRepacker.GUI.Panels.SubPanels  5      6    0HaRepacker.GUI.Panels.SubPanels.AvalonTextEditor   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ?local-clr-namespace:HaRepacker.GUI.Panels.SubPanels     5      ^ ZICSharpCode.AvalonEdit, Version=6.1.2.30, Culture=neutral, PublicKeyToken=9cc39be672370310?
avalonEdit.http://icsharpcode.net/sharpdevelop/avalonedit  5      .��5      �   � RowDefinitions  5   
   �� 5      $
��26px �6   �� 5      $��* �6   .+�5   
   � $��0��6   
�������.+�5      ��  �� Orientation$ 
Horizontal=�6   $��5,0,0,0q�62   .+�5      �� $��Syntax highlighting: ��6   
�������$��Center=�6L   �� 5      -   5         ��Name$% comboBox_SyntaxHighlightingType��5      $��1��5      $��50��5      $	��150��6   $��Center=�5      $
��Left=�6#    �� DisplayMemberPath$
 Name��5      ��5      �� *HighlightingDefinitions+ �� Mode$ OneWay=� �� UpdateSourceTrigger$ PropertyChanged=� �� SelectedItem 5      �� *HighlightingDefinition+$ PropertyChanged=�5      5      �� 5!      -   5         ��Name$ button_saveApply��5!      ���	#FFD1D1D1
���   �$��0,0,5,0q�5"   ?   $
��Save��5#      $	��150��6$   $��Right=�5$      $��Center=�61   {�.  5'      5(      '  !ICSharpCode.AvalonEdit.TextEditor  5*   
   -   5          Name$ 
textEditor��5*       $��1��5+       $��Consolas)�5-       $
��12pt(�66   	   WordWrap	 . 
   ShowLineNumbers
 .    SyntaxHighlighting 5,       �� *HighlightingDefinition+5/       51      52      