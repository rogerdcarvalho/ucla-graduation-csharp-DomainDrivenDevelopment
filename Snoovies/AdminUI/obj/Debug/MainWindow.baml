   M S B A M L   `   `   `  ����B  >AdminUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:AdminUIAdminUI  5   	   7 clr-namespace:AdminUI.PropertiesAdminUI.Properties  5   	       AdminUI.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   'localclr-namespace:AdminUI     5   	   1prop clr-namespace:AdminUI.Properties     5   	     � Title$  Snoovies Administration��5   	   $	��350��6)   $	��525��66   .��5      �  � RowDefinitions 5   
   �� 5	      $
��Auto �5
      �� 5      $��* �5      .+�5   
   � $��0��6    � ColumnDefinitions 5      �� 5      $��5* �5      �� 5      $��5* �5       5      �� 5      $
��Auto �5      �� 5      $
��Auto �5      �� 5      $
��Auto �5      �� 5      $
��Auto �5      .+�5"      �� $��0��5#      $��0��5$      $��Resources/logo.png��5&      $
��None=�6-   $
��Left=�6<   $��	20,10,0,0q�6W   �� 5'      -   5         ��Name$ 	SearchBox��5'      $��0��5(      $��1��5)      $��23��5*       �� TextWrapping$
 Wrap=�6   "   AdminUI.Properties.Resources   SearchFieldText#��Z 61   $��Right=�6b   $��0,0,9,0q�6~   $	��200��6�   6�   �� 5+      -   5         ��Name$ CategorySelector��5/      $��1��5,      $��2��5-      $��1q�5/   `   $��	10,0,10,5q�6t   6�   50      � 51   
   $��2��6    53      �� 54      $
��8.5* �55      �� 56      $
��1.5* �57       59      �� 5:      $��* �5;      .+�5?      �� -   5         ��Name$ 	FilmTable��5@      $��1��6    �� DisplayMemberPath$ Title��5A      $��	10,5,0,10q�6U   5B      �� 5E      $��1��5F      .+�5H      �� -   5        	 ��Name$	 	AddButton��5H      
   AddFilmButton#��Z
 5I      $��Right=�6C   $��0,5,10,5q�6_   6q   �� 5J      -   5        $	 
EditButton��5J         EditFilmButton#��Z 5K      $��Right=�6D   $��0,5,10,5q�6`   6r   5M      5N      5O      5P      