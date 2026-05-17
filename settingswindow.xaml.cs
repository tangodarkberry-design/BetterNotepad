using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace BetterNotepad
{
    public partial class SettingsWindow : Window
    {
        private readonly MainWindow ownerWindow;
        private readonly AppSettings settings;
        private readonly List<EditorTheme> themes;

        public SettingsWindow(MainWindow owner, AppSettings currentSettings, List<EditorTheme> availableThemes)
        {
            InitializeComponent();

            ownerWindow = owner;
            settings = currentSettings;
            themes = availableThemes;

            ThemeBox.ItemsSource = themes.Select(t => t.Name).ToList();
            ThemeBox.SelectedItem = settings.ThemeName;

            FontSizeSlider.Value = settings.FontSize;
            FontSizeText.Text = "Font size: " + settings.FontSize;

            AutoSaveBox.IsChecked = settings.AutoSave;
            LineNumbersBox.IsChecked = settings.ShowLineNumbers;

            FontSizeSlider.ValueChanged += (_, _) =>
            {
                FontSizeText.Text = "Font size: " + (int)FontSizeSlider.Value;
            };
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            settings.ThemeName = ThemeBox.SelectedItem?.ToString() ?? "Better Dark";
            settings.FontSize = (int)FontSizeSlider.Value;
            settings.AutoSave = AutoSaveBox.IsChecked == true;
            settings.ShowLineNumbers = LineNumbersBox.IsChecked == true;

            SettingsStorage.Save(settings);
            ownerWindow.ApplyUserSettings(settings);

            Close();
        }

        private void CancelSettings(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenThemesFolder(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = ThemeManager.ThemeFolder,
                UseShellExecute = true
            });
        }
    }
}