using Avalonia.Controls;
using Avalonia.Media;
using System.IO;
using System;
using Avalonia.Platform.Storage;
using System.Diagnostics;

namespace Roblox_Settings_Modifier;

public partial class MainWindow : Window
{
    const int minWindowSizeX = 640;
    const int minWindowSizeY = 360;
    string selectableFilePath = "C:\\Users\\[Your User]\\AppData\\Local\\Roblox\\GlobalBasicSettings_13.xml";

    public MainWindow()
    {
        InitializeComponent();
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
    }

    public int FPS = 60;
    public int GraphicsLevel = 10;
    public int VolumeLevel = 10;
    public bool Fullscreen = false;
    public int windowSizeX = minWindowSizeX;
    public int windowSizeY = minWindowSizeY;

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
            FilePathTextBox.Text = filePath;
        }
    }
    public void GraphicsLevelSlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (sender is Slider slider)
        {
            GraphicsLevel = (int)slider.Value;
            GraphicsLevelTextBlock.Text = $"Graphics Level: {GraphicsLevel}";

        }
    }

    public void VolumeLevelSlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (sender is Slider slider)
        {
            VolumeLevel = (int)slider.Value;
            VolumeLevelTextBlock.Text = $"Volume Level: {VolumeLevel}";
        }
    }

    public void ChangeFullScreen(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            Fullscreen = !Fullscreen;
            if (Fullscreen)
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
            windowSizeX = (int)slider.Value;
            UpdateWindowSizeTextBlock();
        }
    }

    public void WindowSizeYSlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (sender is Slider slider)
        {
            windowSizeY = (int)slider.Value;
            UpdateWindowSizeTextBlock();
        }
    }

    public void SettingsButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        StatusMessage.Text = "Settings have been applied!";
    }

    private void UpdateWindowSizeTextBlock()
    {
        WindowSizeTextBlock.Text = $"Window Size: {windowSizeX}x{windowSizeY}";
    }
}