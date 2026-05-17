[Setup]
AppName=BetterNotepad
AppVersion=1.0.0
AppPublisher=BetterNotepad
DefaultDirName={autopf}\BetterNotepad
DefaultGroupName=BetterNotepad
OutputDir=C:\BetterNotepad\InstallerOutput
OutputBaseFilename=BetterNotepadSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
SetupIconFile=C:\BetterNotepad\Assets\BetterNotepad.ico
UninstallDisplayIcon={app}\BetterNotepad.exe
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "C:\BetterNotepad\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\BetterNotepad"; Filename: "{app}\BetterNotepad.exe"
Name: "{autodesktop}\BetterNotepad"; Filename: "{app}\BetterNotepad.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\BetterNotepad.exe"; Description: "Launch BetterNotepad"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"