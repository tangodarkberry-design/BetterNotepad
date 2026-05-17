using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace BetterNotepad
{
    public static class FileContextMenuInstaller
    {
        public static void Install()
        {
            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";

            if (string.IsNullOrEmpty(exePath))
                return;

            if (!exePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                return;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string textIcon = Path.Combine(baseDir, "Assets", "FileText.ico");
            string codeIcon = Path.Combine(baseDir, "Assets", "FileCode.ico");
            string htmlIcon = Path.Combine(baseDir, "Assets", "FileHtml.ico");

            RegisterFileType(
                ".txt",
                "BetterNotepad.txt",
                "BetterNotepad Text File",
                $"\"{exePath}\" \"%1\"",
                textIcon
            );

            RegisterFileType(
                ".html",
                "BetterNotepad.html",
                "BetterNotepad HTML File",
                $"\"{exePath}\" \"%1\" --code",
                htmlIcon
            );

            RegisterFileType(
                ".htm",
                "BetterNotepad.html",
                "BetterNotepad HTML File",
                $"\"{exePath}\" \"%1\" --code",
                htmlIcon
            );

            RegisterFileType(
                ".cs",
                "BetterNotepad.code",
                "BetterNotepad Code File",
                $"\"{exePath}\" \"%1\" --code",
                codeIcon
            );

            RegisterFileType(
                ".cpp",
                "BetterNotepad.code",
                "BetterNotepad Code File",
                $"\"{exePath}\" \"%1\" --code",
                codeIcon
            );

            RegisterFileType(
                ".py",
                "BetterNotepad.code",
                "BetterNotepad Code File",
                $"\"{exePath}\" \"%1\" --code",
                codeIcon
            );

            RegisterFileType(
                ".js",
                "BetterNotepad.code",
                "BetterNotepad Code File",
                $"\"{exePath}\" \"%1\" --code",
                codeIcon
            );

            CreateRightClickMenu(".txt", "Open with BetterNotepad", $"\"{exePath}\" \"%1\"");
            CreateRightClickMenu(".html", "Open with CodeEditor", $"\"{exePath}\" \"%1\" --code");
            CreateRightClickMenu(".htm", "Open with CodeEditor", $"\"{exePath}\" \"%1\" --code");
            CreateRightClickMenu(".cs", "Open with CodeEditor", $"\"{exePath}\" \"%1\" --code");
            CreateRightClickMenu(".cpp", "Open with CodeEditor", $"\"{exePath}\" \"%1\" --code");
            CreateRightClickMenu(".py", "Open with CodeEditor", $"\"{exePath}\" \"%1\" --code");
            CreateRightClickMenu(".js", "Open with CodeEditor", $"\"{exePath}\" \"%1\" --code");
        }

        private static void RegisterFileType(
            string extension,
            string progId,
            string description,
            string command,
            string iconPath)
        {
            using RegistryKey? extKey = Registry.CurrentUser.CreateSubKey(
                $@"Software\Classes\{extension}");

            if (extKey == null)
                return;

            extKey.SetValue("", progId);

            using RegistryKey? progKey = Registry.CurrentUser.CreateSubKey(
                $@"Software\Classes\{progId}");

            if (progKey == null)
                return;

            progKey.SetValue("", description);

            using RegistryKey? iconKey = progKey.CreateSubKey("DefaultIcon");

            if (iconKey != null && File.Exists(iconPath))
                iconKey.SetValue("", $"\"{iconPath}\"");

            using RegistryKey? commandKey = progKey.CreateSubKey(
                @"shell\open\command");

            if (commandKey != null)
                commandKey.SetValue("", command);
        }

        private static void CreateRightClickMenu(string ext, string name, string command)
        {
            string path = $@"Software\Classes\SystemFileAssociations\{ext}\shell\BetterNotepad";

            using RegistryKey? key = Registry.CurrentUser.CreateSubKey(path);

            if (key == null)
                return;

            key.SetValue("", name);

            using RegistryKey? cmd = key.CreateSubKey("command");

            if (cmd == null)
                return;

            cmd.SetValue("", command);
        }
    }
}