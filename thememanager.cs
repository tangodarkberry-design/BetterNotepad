using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BetterNotepad
{
    public static class ThemeManager
    {
        public static string ThemeFolder =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes");

        public static List<EditorTheme> LoadThemes()
        {
            Directory.CreateDirectory(ThemeFolder);
            CreateExampleThemesIfMissing();

            List<EditorTheme> themes = new List<EditorTheme>();

            foreach (string file in Directory.GetFiles(ThemeFolder, "*.bntheme"))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    EditorTheme? theme = JsonSerializer.Deserialize<EditorTheme>(json);

                    if (theme != null && !string.IsNullOrWhiteSpace(theme.Name))
                        themes.Add(theme);
                }
                catch
                {
                }
            }

            if (themes.Count == 0)
            {
                themes.Add(new EditorTheme
                {
                    Name = "Better Dark"
                });
            }

            return themes;
        }

        private static void CreateExampleThemesIfMissing()
        {
            string normalPath = Path.Combine(ThemeFolder, "ExampleNormal.bntheme");
            string imagePath = Path.Combine(ThemeFolder, "ExampleImageTheme.bntheme");

            if (!File.Exists(normalPath))
            {
                EditorTheme normal = new EditorTheme
                {
                    Name = "Example Normal",
                    WindowBackground = "#1E1E1E",
                    MenuBackground = "#D9D9D9",
                    MenuText = "#000000",
                    EditorBackground = "#232323",
                    EditorText = "#DCDCDC",
                    EditorBackgroundImage = "",
                    EditorBackgroundImageStretch = "UniformToFill",
                    EditorBackgroundImageOpacity = 0.35,
                    StatusBackground = "#E5E5E5",
                    StatusText = "#000000",
                    LineNumberBackground = "#F0F0F0",
                    LineNumberText = "#000000",
                    OutputBackground = "#DDDDDD",
                    OutputHeaderBackground = "#CCCCCC",
                    OutputText = "#000000"
                };

                File.WriteAllText(normalPath, JsonSerializer.Serialize(normal, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
            }

            if (!File.Exists(imagePath))
            {
                EditorTheme imageTheme = new EditorTheme
                {
                    Name = "Example Image Theme",
                    WindowBackground = "#1E1E1E",
                    MenuBackground = "#D9D9D9",
                    MenuText = "#000000",
                    EditorBackground = "#111111",
                    EditorText = "#FFFFFF",

                    // Drag an image into the Themes folder,
                    // then replace this with the exact image filename.
                    EditorBackgroundImage = "example-background.png",

                    EditorBackgroundImageStretch = "UniformToFill",
                    EditorBackgroundImageOpacity = 0.25,

                    StatusBackground = "#E5E5E5",
                    StatusText = "#000000",
                    LineNumberBackground = "#F0F0F0",
                    LineNumberText = "#000000",
                    OutputBackground = "#DDDDDD",
                    OutputHeaderBackground = "#CCCCCC",
                    OutputText = "#000000"
                };

                File.WriteAllText(imagePath, JsonSerializer.Serialize(imageTheme, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
            }
        }
    }
}