using Avalonia.Controls;
using Avalonia.Media;
using System.IO;
using System;
using Avalonia.Platform.Storage;
using System.Xml;
using System.Threading.Tasks;

namespace Roblox_Settings_Editor;

public partial class MainWindow : Window
{
    AppSettings settings = new AppSettings();
    const int minWindowSizeX = 816; // 640;
    const int minWindowSizeY = 638; // 360;
    string selectableFilePath = "C:\\Users\\[Your User]\\AppData\\Local\\Roblox\\GlobalBasicSettings_13.xml";

    public MainWindow()
    {
        InitializeComponent();

        HeaderPanel.PointerPressed += (s, e) =>
{
    if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
    {
        (VisualRoot as Window)!.BeginMoveDrag(e);
    }
};

        if (OperatingSystem.IsWindows())
        {
            var robloxSettingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Roblox",
                "GlobalBasicSettings_13.xml"
            );

            if (System.IO.File.Exists(robloxSettingsPath))
            {
                selectableFilePath = robloxSettingsPath;
            }
            FilePathSelectableTextBox.Text = "You can find your settings file at " + selectableFilePath;
        }
        else
        {
            FilePathSelectableTextBox.Text = "You can find your settings file at... wait... you're not on Windows!";
        }

        WindowSizeXSlider.Minimum = minWindowSizeX;
        WindowSizeYSlider.Minimum = minWindowSizeY;
        WindowSizeXSlider.Maximum = Screens.Primary.Bounds.Width;
        WindowSizeYSlider.Maximum = Screens.Primary.Bounds.Height;

        if (settings.LoadSettings())
        {
            FpsInput.Text = settings.FPS.ToString();
            FilePathTextBox.Text = settings.FilePath;
            GraphicsLevelSlider.Value = settings.GraphicsLevel;
            VolumeLevelSlider.Value = settings.VolumeLevel;
            WindowSizeXSlider.Value = settings.windowSizeX;
            WindowSizeYSlider.Value = settings.windowSizeY;
            if (settings.Fullscreen)
            {
                FullScreenButton.Content = "Fullscreen: On";
                FullScreenButton.Background = Brushes.SeaGreen;
            }
            else
            {
                FullScreenButton.Content = "Fullscreen: Off";
                FullScreenButton.Background = Brushes.SlateGray;
            }
            StatusMessage.Text = "Settings loaded successfully!";


            if (!string.IsNullOrEmpty(settings.FilePath) && File.Exists(settings.FilePath))
            {
                StartFileWatcher(settings.FilePath); // Add this
            }
        }
    }

    private FileSystemWatcher? fileWatcher;
    private bool isApplyingSettings = false; // Add this flag

    private void StartFileWatcher(string filePath)
    {
        fileWatcher?.Dispose();

        try
        {
            var directory = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);

            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
                return;

            fileWatcher = new FileSystemWatcher(directory)
            {
                Filter = fileName,
                NotifyFilter = NotifyFilters.LastWrite
            };

            fileWatcher.Changed += async (s, e) =>
            {
                // IGNORE if we're the ones writing
                if (isApplyingSettings) return;

                await Task.Delay(500);

                // Double-check we're not writing
                if (isApplyingSettings) return;

                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    ApplySettings();
                    StatusMessage.Text = "File changed - settings reapplied!";
                });
            };

            fileWatcher.EnableRaisingEvents = true;
        }
        catch { }
    }

    private bool ApplySettings()
    {
        // Validate file path
        if (string.IsNullOrEmpty(FilePathTextBox.Text))
        {
            StatusMessage.Text = "Please select a settings file first!";
            return false;
        }

        if (!File.Exists(FilePathTextBox.Text))
        {
            StatusMessage.Text = "File not found!";
            return false;
        }

        // Validate FPS input
        if (settings.FPS < -1)
        {
            StatusMessage.Text = "Invalid FPS value!";
            return false;
        }

        XmlDocument xmlDoc = new XmlDocument();
        try
        {
            isApplyingSettings = true; // SET FLAG BEFORE WRITING
            xmlDoc.Load(FilePathTextBox.Text);

            // Get the Properties node
            XmlNode? propertiesNode = xmlDoc.SelectSingleNode("//Item[@class='UserGameSettings']/Properties");

            if (propertiesNode == null)
            {
                StatusMessage.Text = "Invalid Roblox settings file!";
                return false;
            }

            // Helper function to update a node
            void UpdateNode(string xpath, string value, XmlNode? parentNode = null)
            {
                XmlNode? node = (parentNode ?? propertiesNode).SelectSingleNode(xpath);
                if (node != null)
                {
                    node.InnerText = value;
                }
            }

            // Update all settings
            UpdateNode("int[@name='FramerateCap']", settings.FPS.ToString());
            UpdateNode("int[@name='GraphicsQualityLevel']", settings.GraphicsLevel.ToString());
            UpdateNode("float[@name='MasterVolume']", (settings.VolumeLevel / 10.0f).ToString("0.000000000"));
            UpdateNode("bool[@name='Fullscreen']", settings.Fullscreen.ToString().ToLower());

            // Window size (uses parent parameter)
            XmlNode? windowSizeNode = propertiesNode.SelectSingleNode("Vector2[@name='StartScreenSize']");
            if (windowSizeNode != null)
            {
                UpdateNode("X", settings.windowSizeX.ToString(), windowSizeNode);
                UpdateNode("Y", settings.windowSizeY.ToString(), windowSizeNode);
            }

            // Save
            xmlDoc.Save(FilePathTextBox.Text);
            // Wait a bit before clearing flag
            Task.Delay(1000).ContinueWith(_ => isApplyingSettings = false);
            StatusMessage.Text = "Settings applied successfully!";
            return true;
        }
        catch (Exception ex)
        {
            isApplyingSettings = false; // Clear flag on error
            StatusMessage.Text = $"Error: {ex.Message}";
            return false;
        }
    }

    public void MinimizeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    public void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Close();
    }

    public void CopyPathButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Clipboard!.SetTextAsync(selectableFilePath);
        StatusMessage.Text = "Path copied to clipboard!";
    }

    private async void BrowseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Roblox Settings File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
            new FilePickerFileType("XML Files")
            {
                Patterns = new[] { "*.xml" }
            }
        }
        });

        if (files.Count > 0)
        {
            var filePath = files[0].Path.LocalPath;
            settings.FilePath = filePath;
            FilePathTextBox.Text = settings.FilePath;
            StartFileWatcher(filePath); // Add this line
        }
    }

    public void FpsInputTextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (int.TryParse(textBox.Text, out int fps))
            {
                settings.FPS = fps;
            }
        }
    }

    public void GraphicsLevelSlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (sender is Slider slider)
        {
            settings.GraphicsLevel = (int)slider.Value;
            GraphicsLevelTextBlock.Text = $"Graphics Level: {settings.GraphicsLevel}";

        }
    }

    public void VolumeLevelSlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (sender is Slider slider)
        {
            settings.VolumeLevel = (int)slider.Value;
            VolumeLevelTextBlock.Text = $"Volume Level: {settings.VolumeLevel}";
        }
    }

    public void ChangeFullScreen(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            settings.Fullscreen = !settings.Fullscreen;
            if (settings.Fullscreen)
            {
                button.Content = "Fullscreen: On";
                button.Background = Brushes.SeaGreen;
            }
            else
            {
                button.Content = "Fullscreen: Off";
                button.Background = Brushes.SlateGray;
            }
        }
    }

    public void WindowSizeXSlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (sender is Slider slider)
        {
            settings.windowSizeX = (int)slider.Value;
            UpdateWindowSizeTextBlock();
        }
    }

    public void WindowSizeYSlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (sender is Slider slider)
        {
            settings.windowSizeY = (int)slider.Value;
            UpdateWindowSizeTextBlock();
        }
    }

    public void SettingsButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ApplySettings() && settings.SaveSettings())
        {
            StatusMessage.Text = "Settings have been applied and saved!";
        }
    }

    private void UpdateWindowSizeTextBlock()
    {
        WindowSizeTextBlock.Text = $"Window Size: {settings.windowSizeX}x{settings.windowSizeY}";
    }
}