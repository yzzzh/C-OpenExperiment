   M S B A M L   `   `   `  ����A  =WpfApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:WpfAppWpfApp  5   	       WpfApp.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   &localclr-namespace:WpfApp     5   	     � Title$!  判题系统 - by 尹泽豪��5   	   $	��450��6   $	��525��6,    � WindowStartupLocation$ CenterScreen=�68   .��5	      � .+�5
   
   �� .+�5      �� -   5         ��Name$ label_ip��5      $��IP��6&   �� 5      -   5         ��Name$ ip_text��5      $��23��6'    �� TextWrapping$
 Wrap=�63   $�� ��6G   �� 5      -   5        $ 
label_port��5      $
��Port��6(   �� 5      -   5        $ 	port_text��5      $��23��6)   $
 Wrap=�65   $�� ��6I   �� 5      -   5        $ label_input��5      $��Input��6)   �� 5      -   5        $ 
input_text��5      $��23��6*   $
 Wrap=�66   $�� ��6J   �� 5      -   5        $ label_output��5      $��Output��6*   �� 5      -   5        $ output_text��5      $��22��6+   $
 Wrap=�67   $�� ��6K   �� 5      -	   5        $ label_source_code��5      $��Source Code��6/   �� 5      -
   5        $ source_code_text��5      $	��100��60   $
 Wrap=�6=    �� AcceptsReturn . " �� VerticalScrollBarVisibility$ Visible=�6f   $�� ��6�   �� 5      -   5        $ label_response��5      $��Response��6,   �� 5      -   5        $ response_text��5      $��40��6-   $
 Wrap=�69   $�� ��6M   �� 5      -   5         ��Name$ button_send��5      $
��Send��6*   69   5      5      5      