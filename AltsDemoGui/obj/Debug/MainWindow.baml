   M S B A M L   `   `   `  ����F  BAltsDemoGui, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:AltsDemoGuiAltsDemoGui  5   	       AltsDemoGui.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   +localclr-namespace:AltsDemoGui     5   	     � Title$  
MainWindow��5   	   $	��450��6   $	��800��6)    � DataContext 5	        AltsDemoGui.ViewModel  5
   
   .��5      � .+�5   
   �� -   5        $��Connect��5      $
��Left=�6#   $��
10,100,0,0q�6>   $	��Top=�6R   $��75��6j   6u   �� 5   
   -   5        $��
Disconnect��5      $
��Left=�6&   $��
10,130,0,0q�6A   $	��Top=�6U   $��75��6m   6x   �� 5   
   -   5        $��Learn��5      $
��Left=�6!   $��
10,160,0,0q�6<   $	��Top=�6P   $��75��6h   6s   A� 5   
   $
��Left=�6<   $��38��6W   $��143,118,0,0q�6c   
���   �$	��Top=�6�   $��41��6�   ��6   �� *StatusColor+ �� Mode$ OneWay=�6�   �� 5   
   $
��Left=�6   $	��320��6-   $��
295,46,0,0q�6:    �� TextWrapping$
 Wrap=�6N   $	��Top=�6�   $	��408��6�   ��6b   �� *	LogText+$ OneWay=�6�   5      5      