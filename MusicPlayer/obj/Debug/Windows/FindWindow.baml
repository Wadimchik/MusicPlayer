   M S B A M L   `   `   `  ����F  BMusicPlayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:MusicPlayerMusicPlayer  5   	       MusicPlayer.FindWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   _ [Microsoft.Xaml.Behaviors, Version=1.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a3b+http://schemas.microsoft.com/xaml/behaviors  5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   +localclr-namespace:MusicPlayer     5   	   -   5        $��Window��5   *     � Title$  
Поиск��5	   	    � 
ResizeMode$ NoResize=�6    � SizeToContent$ WidthAndHeight=�6-    � WindowStartupLocation$ CenterOwner=�6L   .��5       � DataContext 5      &    MusicPlayer.FindWindow_ViewModel  5   
   .��5      �� .+�5   
   ]� $��15, 0, 15, 0q�6   .+�5      �� $��Имя трека:��6   $	��130��6)   $��Right=�65   $��
0, 5, 0, 0q�6X   �� 5      $	��150��6,   $
��Left=�68   $��
5, 5, 0, 0q�6S   ��6   �� *Name+.��6g   *  $Microsoft.Xaml.Behaviors.Interaction   Triggers 5      +  %Microsoft.Xaml.Behaviors.EventTrigger  5         	EventName$ TextChanged��6%   *  $Microsoft.Xaml.Behaviors.TriggerBase   Actions. 5      2  ,Microsoft.Xaml.Behaviors.InvokeCommandAction     PassEventArgsToCommand . 	   Command	 60   �� *TextChangedCommand+6W   5!      .��5#   *   5%      5&      ]� 5(   
   $��Center=�6   .+�5*      �� $	��110��6k   $
��Left=�6w   $��0, 15, 5, 15q�6�   ��6   �� *FindButtonCommand+��6;   �� 
 �� ElementName$
 Window��.��5,      ]� .+�5.      �� $ ��/Resources/Images/Find.png��6   $��16��6@   $��16��6K   $��
0, 0, 5, 0q�6W   �� 5/      $��
Найти��6    51      53      �� 55       �� IsCancel . $	��110��6%   $
��Left=�61   $��5, 15, 0, 15q�6L   .��57      ]� .+�59      �� $"��/Resources/Images/Cancel.png��6   $��16��6B   $��16��6M   $��
0, 0, 5, 0q�6Y   �� 5:      $��Отменить��6    5<      5>      5@      5B      5D      