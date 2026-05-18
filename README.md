# BetterNotepad

BetterNotepad is a modern Windows text editor and lightweight code editor built with WPF and .NET.
It combines classic notepad simplicity with advanced editing tools, extension support, image viewing, folder management, and built-in developer utilities.

---

## Features

* Multi-tab text and code editing
* Syntax highlighting
* Lua, C#, Python, HTML, JavaScript, and more
* Image and video viewing support
* Custom themes and font settings
* HTML live preview
* Folder manager support
* Extension system using DLL plugins
* Modern dark UI
* "Open With BetterNotepad" Windows integration
* Self-contained Windows builds

---

## Included Extensions

### BetterPhoto

Advanced image editing extension featuring:

* Drawing and painting tools
* Brush types
* Eraser tool
* Undo support
* Zoom controls
* Brightness adjustment
* Contrast adjustment
* Saturation controls
* Warm/Cool filters
* Vintage presets
* Export edited images

---

### BetterTerminal

Integrated PowerShell terminal extension featuring:

* Built-in PowerShell execution
* Live command output
* Console-style interface
* Dark themed terminal UI
* Quick command testing inside BetterNotepad

---

### FolderManager

Folder browsing extension featuring:

* Open folders directly in BetterNotepad
* Sidebar-style folder explorer
* File tree navigation
* Double-click to open files
* Explorer integration
* Folder startup support

---

## Extension System

BetterNotepad supports custom extensions loaded from:

```text
%AppData%\BetterNotepad\Extensions
```

Extensions can:

* Add custom menus
* Create new windows
* Open tools
* Access images/files
* Integrate directly into the editor

This allows developers to create their own plugins without modifying the BetterNotepad source code.

---

## Built With

* C#
* .NET 8
* WPF
* AvalonEdit

---

## Status

BetterNotepad is actively being developed and expanded with new extensions, tools, and customization features.
