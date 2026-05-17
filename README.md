BetterNotepad

A modern multi-purpose Windows editor built with WPF and C#.

BetterNotepad started as a lightweight notepad replacement, but evolved into a powerful editor capable of handling code, images, videos, themes, HTML previews, tabs, and more — all inside a clean desktop application.

And i made sure that copilot is non-exsistant :3, also im going to be adding more stuff to it and im open to suggestions :]

Features
Modern tabbed interface
BetterNotepad mode
CodeEditor mode
Syntax highlighting
HTML live preview
Image viewer
Video player
Drag & drop file support
Autosave
Custom themes
Image background themes
Explorer context menu integration
Single-window multi-tab workflow
File type detection
Built with WPF + .NET 8
Supported File Types
Text / Code
.txt
.html
.htm
.cs
.cpp
.h
.js
.py
.lua
Images
.png
.jpg
.jpeg
.bmp
.gif
.tif
.tiff
.webp
.ico
.nef (partial support depending on Windows codecs)
Videos
.mp4
.wmv
.avi
.mov
.mkv
.webm
.m4v
Screenshots

Add screenshots here later.

Themes

BetterNotepad supports fully customizable themes.

Themes are stored as:

Themes/*.bntheme

Themes can:

change colors
change text colors
customize menus
customize status bars
use image backgrounds
control image opacity/stretching

Example:

{
  "Name": "My Theme",
  "EditorBackground": "#232323",
  "EditorText": "#FFFFFF",
  "EditorBackgroundImage": "background.png",
  "EditorBackgroundImageOpacity": 0.25
}

HTML Live Preview

When editing HTML files in CodeEditor mode:

View -> Output

opens a live HTML preview beside the editor.

Installation

Download the latest installer from Releases:

BetterNotepadSetup.exe

Run the installer and launch BetterNotepad.

Build:

dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true
Project Structure
BetterNotepad/
├── Assets/
├── Themes/
├── Syntax/
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── ThemeManager.cs
├── FileContextMenuInstaller.cs
└── BetterNotepad.csproj
Planned Features
Better syntax highlighting
Plugin system
LAN messaging
Markdown preview
Audio support
Multi-monitor support
Custom startup layouts
Terminal integration
Git integration
License

MIT License

Credits

Built by TANGO using:

- C#
- WPF
- AvalonEdit
- .NET 8
