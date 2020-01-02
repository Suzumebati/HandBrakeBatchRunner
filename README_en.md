# About HandBrake Batch Runner
It is an application that converts video files together using HandbreakCLI.

Handbreak is a very good video converter. However, it is a little inconvenient when you want to convert them all together. In order to solve this, it becomes a tool to convert videos conveniently by continuously executing what is specified in the GUI.

I made it with VB.NET + WinForm for personal use, but I want to study, so I will make it again with .NET Core + WPF.

# How to use
![operation](https://user-images.githubusercontent.com/51582636/71642448-a331a600-2cee-11ea-9957-fcb2422b36db.gif)
1. Convert settings etc. first (change the command template of HandbrakeCLI)
2. Set file conversion destination
3. Drag and drop the files you want to convert
4. Click Start Conversion

# Useful function
* If you specify the conversion completed folder, the converted file will be moved
* Skip conversion if there is already a conversion file with the same name (double conversion prevention)
* Next, if you cancel with the cancel button, the file being converted will be canceled after completion
* Files can be added and deleted even during conversion
* Files you want to convert can be saved with the save function and restarted (files that have been converted are skipped)

# To be implemented
* Multi language
* Monitoring of specific folders, automatic conversion mode
* Material design
