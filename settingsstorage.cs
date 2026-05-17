using System;
using System.IO;
using System.Text.Json;

namespace BetterNotepad
{
    public static class SettingsStorage
    {
        private static string Folder =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BetterNotepad");

        private static string FilePath =>
            Path.Combine(Folder, "settings.json");

        public static AppSettings Load()
        {
            try
            {
                Directory.CreateDirectory(Folder);

                if (!File.Exists(FilePath))
                    return new AppSettings();

                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public static void Save(AppSettings settings)
        {
            try
            {
                Directory.CreateDirectory(Folder);

                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(FilePath, json);
            }
            catch
            {
            }
        }
    }
}