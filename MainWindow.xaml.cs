using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace BetterNotepad
{
    public partial class MainWindow : Window
    {
        private OpenDocument? currentDocument;

        private bool isCodeMode = false;
        private bool isHtmlOutputVisible = false;
        private bool isImageMode = false;
        private bool isVideoMode = false;
        private bool isLoadingTab = false;

        private string currentLanguage = "Text";

        private AppSettings settings;
        private List<EditorTheme> themes;

        public MainWindow(string[] startupArgs)
        {
            InitializeComponent();

            settings = SettingsStorage.Load();
            themes = ThemeManager.LoadThemes();

            try
            {
                FileContextMenuInstaller.Install();
            }
            catch
            {
            }

            HandleExternalArgs(startupArgs);
            ApplyUserSettings(settings);

            if (Tabs.Items.Count == 0)
                CreateNewTextTab();
        }

        public void HandleExternalArgs(string[] args)
        {
            bool wantsCodeMode = args.Any(a => a.Equals("--code", StringComparison.OrdinalIgnoreCase));

            string? fileArg = args
                .Where(a => !a.Equals("--code", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault(a =>
                    File.Exists(a) &&
                    Path.GetExtension(a).ToLower() != ".dll" &&
                    Path.GetExtension(a).ToLower() != ".exe");

            if (!string.IsNullOrEmpty(fileArg))
            {
                OpenFileInTab(fileArg, wantsCodeMode);
                BringWindowToFront();
                return;
            }

            if (wantsCodeMode)
                SetCodeMode();
            else if (currentDocument == null)
                SetNotepadMode();

            BringWindowToFront();
        }

        public void ApplyUserSettings(AppSettings newSettings)
        {
            settings = newSettings;
            themes = ThemeManager.LoadThemes();

            Editor.FontSize = settings.FontSize;

            EditorTheme theme =
                themes.FirstOrDefault(t => t.Name == settings.ThemeName)
                ?? themes.FirstOrDefault()
                ?? new EditorTheme();

            ApplyTheme(theme);

            if (isCodeMode && !isImageMode && !isVideoMode)
                Editor.ShowLineNumbers = settings.ShowLineNumbers;
            else
                Editor.ShowLineNumbers = false;
        }

        private void ApplyTheme(EditorTheme theme)
        {
            Background = BrushFromHex(theme.WindowBackground);

            TopMenu.Background = BrushFromHex(theme.MenuBackground);
            TopMenu.Foreground = BrushFromHex(theme.MenuText);

            Editor.Foreground = BrushFromHex(theme.EditorText);

            string imagePath = "";

            if (!string.IsNullOrWhiteSpace(theme.EditorBackgroundImage))
                imagePath = Path.Combine(ThemeManager.ThemeFolder, theme.EditorBackgroundImage);

            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                ImageBrush imageBrush = new ImageBrush
                {
                    ImageSource = bitmap,
                    Opacity = theme.EditorBackgroundImageOpacity,
                    Stretch = GetStretch(theme.EditorBackgroundImageStretch)
                };

                Editor.Background = imageBrush;
            }
            else
            {
                Editor.Background = BrushFromHex(theme.EditorBackground);
            }

            StatusBar.Background = BrushFromHex(theme.StatusBackground);
            Status.Foreground = BrushFromHex(theme.StatusText);
            LangStatus.Foreground = BrushFromHex(theme.StatusText);

            OutputPanel.Background = BrushFromHex(theme.OutputBackground);
            OutputHeader.Background = BrushFromHex(theme.OutputHeaderBackground);
            OutputHeader.Foreground = BrushFromHex(theme.OutputText);
        }

        private SolidColorBrush BrushFromHex(string hex)
        {
            try
            {
                return (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;
            }
            catch
            {
                return Brushes.Gray;
            }
        }

        private Stretch GetStretch(string value)
        {
            return value switch
            {
                "None" => Stretch.None,
                "Fill" => Stretch.Fill,
                "Uniform" => Stretch.Uniform,
                "UniformToFill" => Stretch.UniformToFill,
                _ => Stretch.UniformToFill
            };
        }

        private void BringWindowToFront()
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;

            Activate();
            Topmost = true;
            Topmost = false;
            Focus();
        }

        private void SaveCurrentDocumentState()
        {
            if (currentDocument == null || isLoadingTab)
                return;

            if (currentDocument.Kind == DocumentKind.Text || currentDocument.Kind == DocumentKind.Code)
                currentDocument.Text = Editor.Text;
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Tabs.SelectedItem is not TabItem selectedTab)
                return;

            SaveCurrentDocumentState();

            if (selectedTab.Tag is OpenDocument doc)
                LoadDocument(doc);
        }

        private void LoadDocument(OpenDocument doc)
        {
            isLoadingTab = true;
            currentDocument = doc;

            try
            {
                ExitImageMode();
                ExitVideoMode();

                if (doc.Kind == DocumentKind.Image)
                {
                    OpenImageDocument(doc);
                }
                else if (doc.Kind == DocumentKind.Video)
                {
                    OpenVideoDocument(doc);
                }
                else
                {
                    Editor.Text = doc.Text;

                    if (doc.Kind == DocumentKind.Code)
                        SetCodeMode();
                    else
                        SetNotepadMode();

                    Status.Text = string.IsNullOrEmpty(doc.FilePath)
                        ? "Opened tab: " + doc.Title
                        : "Opened: " + Path.GetFileName(doc.FilePath);

                    UpdateLanguage();
                }

                ApplyUserSettings(settings);
            }
            finally
            {
                isLoadingTab = false;
            }
        }

        private StackPanel BuildTabHeader(OpenDocument doc)
        {
            StackPanel headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            TextBlock titleText = new TextBlock
            {
                Text = doc.Title,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            Button closeButton = new Button
            {
                Content = "X",
                Width = 18,
                Height = 18,
                Padding = new Thickness(0),
                FontSize = 10,
                Margin = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                Tag = doc
            };

            closeButton.Click += CloseTabButton_Click;

            headerPanel.Children.Add(titleText);
            headerPanel.Children.Add(closeButton);

            return headerPanel;
        }

        private void CreateNewTextTab()
        {
            OpenDocument doc = new OpenDocument
            {
                Title = "Untitled",
                Text = "",
                Kind = DocumentKind.Text
            };

            AddDocumentTab(doc);
        }

        private void AddDocumentTab(OpenDocument doc)
        {
            TabItem tab = new TabItem
            {
                Header = BuildTabHeader(doc),
                Tag = doc
            };

            Tabs.Items.Add(tab);
            Tabs.SelectedItem = tab;
        }

        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            if (button.Tag is not OpenDocument doc)
                return;

            TabItem? tabToRemove = null;

            foreach (TabItem tab in Tabs.Items)
            {
                if (tab.Tag == doc)
                {
                    tabToRemove = tab;
                    break;
                }
            }

            if (tabToRemove == null)
                return;

            if (currentDocument == doc)
            {
                ExitImageMode();
                ExitVideoMode();
                currentDocument = null;
            }

            Tabs.Items.Remove(tabToRemove);

            if (Tabs.Items.Count == 0)
                CreateNewTextTab();
        }

        private void OpenFileInTab(string path, bool forceCodeMode = false)
        {
            SaveCurrentDocumentState();

            foreach (TabItem tab in Tabs.Items)
            {
                if (tab.Tag is OpenDocument existing &&
                    !string.IsNullOrEmpty(existing.FilePath) &&
                    string.Equals(existing.FilePath, path, StringComparison.OrdinalIgnoreCase))
                {
                    Tabs.SelectedItem = tab;
                    return;
                }
            }

            OpenDocument doc = new OpenDocument
            {
                FilePath = path,
                Title = Path.GetFileName(path)
            };

            if (IsImageFile(path))
            {
                doc.Kind = DocumentKind.Image;
            }
            else if (IsVideoFile(path))
            {
                doc.Kind = DocumentKind.Video;
            }
            else
            {
                doc.Text = File.ReadAllText(path);

                string ext = Path.GetExtension(path).ToLower();

                if (forceCodeMode || ext == ".html" || ext == ".htm" || ext == ".cs" || ext == ".cpp" || ext == ".h" || ext == ".js" || ext == ".py" || ext == ".lua")
                    doc.Kind = DocumentKind.Code;
                else
                    doc.Kind = DocumentKind.Text;
            }

            AddDocumentTab(doc);
        }

        private void CloseCurrentTab(object sender, RoutedEventArgs e)
        {
            if (Tabs.SelectedItem is not TabItem tab)
                return;

            if (tab.Tag is OpenDocument doc && doc == currentDocument)
            {
                ExitImageMode();
                ExitVideoMode();
                currentDocument = null;
            }

            Tabs.Items.Remove(tab);

            if (Tabs.Items.Count == 0)
                CreateNewTextTab();
        }

        private void SwitchMode(object sender, RoutedEventArgs e)
        {
            if (currentDocument == null)
                return;

            if (currentDocument.Kind == DocumentKind.Image || currentDocument.Kind == DocumentKind.Video)
                return;

            SaveCurrentDocumentState();

            if (currentDocument.Kind == DocumentKind.Code)
                currentDocument.Kind = DocumentKind.Text;
            else
                currentDocument.Kind = DocumentKind.Code;

            LoadDocument(currentDocument);
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            themes = ThemeManager.LoadThemes();

            SettingsWindow settingsWindow = new SettingsWindow(this, settings, themes);
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void SetNotepadMode()
        {
            isCodeMode = false;
            isImageMode = false;
            isVideoMode = false;

            Title = "BetterNotepad";

            Editor.Visibility = Visibility.Visible;
            ImageViewer.Visibility = Visibility.Collapsed;
            VideoViewer.Visibility = Visibility.Collapsed;

            Editor.ShowLineNumbers = false;
            Editor.SyntaxHighlighting = null;

            ViewMenu.Visibility = Visibility.Collapsed;
            HideHtmlOutput();

            Status.Text = "Mode: Notepad";
            LangStatus.Text = "";

            ApplyUserSettings(settings);
        }

        private void SetCodeMode()
        {
            isCodeMode = true;
            isImageMode = false;
            isVideoMode = false;

            Title = "CodeEditor";

            Editor.Visibility = Visibility.Visible;
            ImageViewer.Visibility = Visibility.Collapsed;
            VideoViewer.Visibility = Visibility.Collapsed;

            Editor.ShowLineNumbers = settings.ShowLineNumbers;

            Status.Text = "Mode: Code Editor";

            UpdateLanguage();
            ApplyUserSettings(settings);
        }

        private void SetImageMode()
        {
            isCodeMode = false;
            isImageMode = true;
            isVideoMode = false;

            Title = "BetterNotepad Image Viewer";

            Editor.Visibility = Visibility.Collapsed;
            ImageViewer.Visibility = Visibility.Visible;
            VideoViewer.Visibility = Visibility.Collapsed;

            Editor.ShowLineNumbers = false;
            Editor.SyntaxHighlighting = null;

            ViewMenu.Visibility = Visibility.Collapsed;
            HideHtmlOutput();

            Status.Text = "Mode: Image Viewer";

            if (currentDocument != null)
                LangStatus.Text = "Image: " + Path.GetExtension(currentDocument.FilePath).ToUpper().Replace(".", "");
        }

        private void SetVideoMode()
        {
            isCodeMode = false;
            isImageMode = false;
            isVideoMode = true;

            Title = "BetterNotepad Video Player";

            Editor.Visibility = Visibility.Collapsed;
            ImageViewer.Visibility = Visibility.Collapsed;
            VideoViewer.Visibility = Visibility.Visible;

            Editor.ShowLineNumbers = false;
            Editor.SyntaxHighlighting = null;

            ViewMenu.Visibility = Visibility.Collapsed;
            HideHtmlOutput();

            Status.Text = "Mode: Video Player";

            if (currentDocument != null)
                LangStatus.Text = "Video: " + Path.GetExtension(currentDocument.FilePath).ToUpper().Replace(".", "");
        }

        private bool IsImageFile(string path)
        {
            string ext = Path.GetExtension(path).ToLower();

            return ext == ".png"
                || ext == ".jpg"
                || ext == ".jpeg"
                || ext == ".bmp"
                || ext == ".gif"
                || ext == ".tif"
                || ext == ".tiff"
                || ext == ".webp"
                || ext == ".ico"
                || ext == ".nef";
        }

        private bool IsVideoFile(string path)
        {
            string ext = Path.GetExtension(path).ToLower();

            return ext == ".mp4"
                || ext == ".wmv"
                || ext == ".avi"
                || ext == ".mov"
                || ext == ".mkv"
                || ext == ".webm"
                || ext == ".m4v";
        }

        private void OpenImageDocument(OpenDocument doc)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(doc.FilePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                ImagePreview.Source = bitmap;

                SetImageMode();

                Status.Text = "Opened image: " + Path.GetFileName(doc.FilePath);
            }
            catch
            {
                SetNotepadMode();
                Status.Text = "Image format not supported yet: " + Path.GetExtension(doc.FilePath);
            }
        }

        private void OpenVideoDocument(OpenDocument doc)
        {
            try
            {
                VideoPlayer.Source = new Uri(doc.FilePath);
                VideoPlayer.Play();

                SetVideoMode();

                Status.Text = "Playing video: " + Path.GetFileName(doc.FilePath);
            }
            catch
            {
                SetNotepadMode();
                Status.Text = "Video format not supported: " + Path.GetExtension(doc.FilePath);
            }
        }

        private void ExitImageMode()
        {
            isImageMode = false;
            ImagePreview.Source = null;
            ImageViewer.Visibility = Visibility.Collapsed;
            Editor.Visibility = Visibility.Visible;
        }

        private void ExitVideoMode()
        {
            isVideoMode = false;

            try
            {
                VideoPlayer.Stop();
                VideoPlayer.Source = null;
            }
            catch
            {
            }

            VideoViewer.Visibility = Visibility.Collapsed;
            Editor.Visibility = Visibility.Visible;
        }

        private void PlayVideo(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Play();
            Status.Text = "Video playing";
        }

        private void PauseVideo(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Pause();
            Status.Text = "Video paused";
        }

        private void StopVideo(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Stop();
            Status.Text = "Video stopped";
        }

        private void NewFile(object sender, RoutedEventArgs e)
        {
            SaveCurrentDocumentState();
            CreateNewTextTab();
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Filter =
                "All Supported Files|*.txt;*.html;*.htm;*.cs;*.cpp;*.h;*.js;*.py;*.lua;*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff;*.webp;*.ico;*.nef;*.mp4;*.wmv;*.avi;*.mov;*.mkv;*.webm;*.m4v|" +
                "Code/Text Files|*.txt;*.html;*.htm;*.cs;*.cpp;*.h;*.js;*.py;*.lua|" +
                "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff;*.webp;*.ico;*.nef|" +
                "Video Files|*.mp4;*.wmv;*.avi;*.mov;*.mkv;*.webm;*.m4v|" +
                "All Files|*.*";

            if (dlg.ShowDialog() == true)
                OpenFileInTab(dlg.FileName);
        }

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            if (currentDocument == null)
                return;

            if (currentDocument.Kind == DocumentKind.Image)
            {
                Status.Text = "Images cannot be saved from viewer mode";
                return;
            }

            if (currentDocument.Kind == DocumentKind.Video)
            {
                Status.Text = "Videos cannot be saved from player mode";
                return;
            }

            SaveCurrentDocumentState();

            if (string.IsNullOrEmpty(currentDocument.FilePath))
            {
                SaveFileDialog dlg = new SaveFileDialog();

                if (dlg.ShowDialog() == true)
                {
                    currentDocument.FilePath = dlg.FileName;
                    currentDocument.Title = Path.GetFileName(dlg.FileName);

                    if (Tabs.SelectedItem is TabItem tab)
                        tab.Header = BuildTabHeader(currentDocument);
                }
                else
                {
                    return;
                }
            }

            File.WriteAllText(currentDocument.FilePath, currentDocument.Text);
            Status.Text = "Saved";

            if (isHtmlOutputVisible)
                RefreshHtmlOutput();
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            try
            {
                VideoPlayer.Stop();
            }
            catch
            {
            }

            Application.Current.Shutdown();
        }

        private void Editor_TextChanged(object? sender, EventArgs e)
        {
            if (isLoadingTab || currentDocument == null)
                return;

            if (currentDocument.Kind == DocumentKind.Text || currentDocument.Kind == DocumentKind.Code)
                currentDocument.Text = Editor.Text;

            if (isCodeMode)
                UpdateLanguage();

            if (settings.AutoSave &&
                !string.IsNullOrEmpty(currentDocument.FilePath) &&
                (currentDocument.Kind == DocumentKind.Text || currentDocument.Kind == DocumentKind.Code))
            {
                try
                {
                    File.WriteAllText(currentDocument.FilePath, currentDocument.Text);
                    Status.Text = "Auto-saved";
                }
                catch
                {
                    Status.Text = "Auto-save failed";
                }
            }

            if (isHtmlOutputVisible)
                RefreshHtmlOutput();
        }

        private void UpdateLanguage()
        {
            if (currentDocument == null)
                return;

            if (currentDocument.Kind == DocumentKind.Image || currentDocument.Kind == DocumentKind.Video)
                return;

            string ext = string.IsNullOrEmpty(currentDocument.FilePath) ? "" : Path.GetExtension(currentDocument.FilePath).ToLower();
            string text = Editor.Text.ToLower();

            if (ext == ".html" || ext == ".htm" || text.Contains("<html") || text.Contains("<!doctype html") || text.Contains("<div") || text.Contains("<body"))
                currentLanguage = "HTML";
            else if (ext == ".cs" || text.Contains("using ") || text.Contains("namespace ") || text.Contains("class "))
                currentLanguage = "C#";
            else if (ext == ".cpp" || ext == ".h" || text.Contains("#include") || text.Contains("std::") || text.Contains("cout"))
                currentLanguage = "C++";
            else if (ext == ".py" || text.Contains("def ") || text.Contains("import ") || text.Contains("print("))
                currentLanguage = "Python";
            else if (ext == ".js" || text.Contains("function") || text.Contains("console.log") || text.Contains("let ") || text.Contains("const "))
                currentLanguage = "JavaScript";
            else if (ext == ".lua" || text.Contains("local ") || text.Contains("end") || text.Contains("then"))
                currentLanguage = "Lua";
            else
                currentLanguage = "Text";

            currentDocument.Language = currentLanguage;

            if (isCodeMode)
            {
                LangStatus.Text = "Language: " + currentLanguage;
                ApplySyntaxHighlighting(currentLanguage);
            }

            if (isCodeMode && currentLanguage == "HTML")
                ViewMenu.Visibility = Visibility.Visible;
            else
            {
                ViewMenu.Visibility = Visibility.Collapsed;
                HideHtmlOutput();
            }
        }

        private void ApplySyntaxHighlighting(string language)
        {
            string fileName = language switch
            {
                "HTML" => "html.xshd",
                "C#" => "csharp.xshd",
                "C++" => "cpp.xshd",
                "Python" => "python.xshd",
                "JavaScript" => "javascript.xshd",
                "Lua" => "lua.xshd",
                _ => ""
            };

            if (string.IsNullOrEmpty(fileName))
            {
                Editor.SyntaxHighlighting = null;
                return;
            }

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Syntax", fileName);

            if (!File.Exists(path))
            {
                Editor.SyntaxHighlighting = null;
                Status.Text = "Missing syntax file: " + fileName;
                return;
            }

            try
            {
                using FileStream stream = File.OpenRead(path);
                using XmlReader reader = XmlReader.Create(stream);
                IHighlightingDefinition definition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                Editor.SyntaxHighlighting = definition;
            }
            catch
            {
                Editor.SyntaxHighlighting = null;
                Status.Text = "Syntax load failed: " + fileName;
            }
        }

        private void ToggleHtmlOutput(object sender, RoutedEventArgs e)
        {
            if (currentLanguage != "HTML")
                return;

            if (isHtmlOutputVisible)
                HideHtmlOutput();
            else
                ShowHtmlOutput();
        }

        private void ShowHtmlOutput()
        {
            isHtmlOutputVisible = true;
            OutputPanel.Visibility = Visibility.Visible;
            OutputColumn.Width = new GridLength(1, GridUnitType.Star);
            RefreshHtmlOutput();
            Status.Text = "HTML output visible";
        }

        private void HideHtmlOutput()
        {
            isHtmlOutputVisible = false;
            OutputPanel.Visibility = Visibility.Collapsed;
            OutputColumn.Width = new GridLength(0);
        }

        private void RefreshHtmlOutput()
        {
            if (!isHtmlOutputVisible)
                return;

            try
            {
                HtmlPreview.NavigateToString(Editor.Text);
            }
            catch
            {
                Status.Text = "HTML output failed";
            }
        }

        private void RunScript(object sender, RoutedEventArgs e)
        {
            if (currentDocument == null)
                return;

            if (currentDocument.Kind == DocumentKind.Image)
            {
                Status.Text = "Cannot run an image";
                return;
            }

            if (currentDocument.Kind == DocumentKind.Video)
            {
                Status.Text = "Cannot run a video";
                return;
            }

            if (string.IsNullOrEmpty(currentDocument.FilePath))
            {
                Status.Text = "Save file first";
                return;
            }

            string ext = Path.GetExtension(currentDocument.FilePath).ToLower();

            try
            {
                if (ext == ".py")
                    Process.Start("python", $"\"{currentDocument.FilePath}\"");
                else if (ext == ".js")
                    Process.Start("node", $"\"{currentDocument.FilePath}\"");
                else if (ext == ".lua")
                    Process.Start("lua", $"\"{currentDocument.FilePath}\"");
                else
                    Status.Text = "Unsupported file type";
            }
            catch (Exception ex)
            {
                Status.Text = ex.Message;
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string file in files)
                OpenFileInTab(file);
        }
    }
}