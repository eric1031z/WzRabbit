   M S B A M L   `   `   `  ����P  LHaRepackerResurrected, Version=4.2.4.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:HaRepacker.PropertiesHaRepacker.Properties  5      Q-clr-namespace:HaRepacker.GUI.Panels.SubPanelsHaRepacker.GUI.Panels.SubPanels  5      ;"clr-namespace:HaRepacker.ConverterHaRepacker.Converter  5
      2    ,HaRepacker.GUI.Panels.SubPanels.LoadingPanel   5      )p#clr-namespace:HaRepacker.Properties  5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ?local-clr-namespace:HaRepacker.GUI.Panels.SubPanels     5      8	converter"clr-namespace:HaRepacker.Converter     5
      U QWpfAnimatedGif, Version=2.0.0.0, Culture=neutral, PublicKeyToken=9e7cd3b544a090dc,gif"http://wpfanimatedgif.codeplex.com  5      "  WpfAnimatedGif.ImageBehavior    AnimateInDesignMode  .  � 	Resources 5      7   1HaRepacker.Converter.TicksToRelativeTimeConverter    ticksToRelativeTimeConverter6   0HaRepacker.Converter.IntegerULongCommasConverter  integerULongCommasConverter��%:   &	        &	        5   
   61     5   
   60   5      .��5      � 
������� � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $��40 �6   .+�5   
   �� -   5         ��Name$ imageLoadingGif��5      $	��300��5      $	��300��6   $��0��6,      RepeatBehavior$ Forever��5         AnimatedSource$$ ../../../Resources/loading.gif��5       6T   �� 5"   
   %   HaRepacker.Properties.Resources   Subpanel_Loading#��Z 6   $��25(�6C   
���   �$��1��5#      $��Center=�6!   $��Center=�6>   $��Roboto Mono Medium)�6Y   �� 5&   
   -   5         ��Name$# stackPanel_wzIvBruteforceStat��5&      $y�	Collapsed=�5'       �� Orientation$ Vertical=�5(      $��0��5)      $��Right=�5*      $	��Top=�6)   $��30q�6A   .+�5+      �� $��Wz IV Key bruteforcer��6   �� 5-      $ 
Horizontal=�6   $��10,0,0,0q�62   .+�5.      �� $��Keys tested: ��6   
�������61   �� 5/      
���������6   �� *WzIvKeyTries+	 �� 	Converter#	 [ 6s   �� 50      $��/ ��6   
�������6&   �� 51      $��4,294,967,294��6   
�������61   52      �� 53      $ 
Horizontal=�6   $��10,0,0,0q�62   .+�54      �� $��
Duration: ��6   
�������6/   �� 55      
���������6   �� *WzIvKeyDuration+#	 [  6w   56      57      58      59      