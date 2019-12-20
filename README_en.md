# About HandBrake Batch Runner
It is an application that converts video files together using HandbreakCLI.

Handbreak is a very good video converter. However, it is a little inconvenient when you want to convert them all together. In order to solve this, it becomes a tool to convert videos conveniently by continuously executing what is specified in the GUI.

I made it with VB.NET + WinForm for personal use, but I want to study, so I will make it again with .NET Core + WPF.

# current situation
12/12/2019 github repository creation.  
12/20/2019 I created a project.  
12/20/2019 WPF only window design , and I will manage the implementation schedule and completion there  

# Planned to be implemented
* HandbreakCLI command line options can be saved and selected during conversion
* Multiple files can be specified by drag and drop
* Move to a specific folder after conversion + Check file with same name (prevents second conversion)
* Progress display
* Waiting for conversion completion at multiple startups

# Scheduled to be implemented
* Multi language
* Monitoring specific folders, automatic conversion mode
* Material design
