using Avalonia.Controls;
using Avalonia.Media;
using System.IO;
using System;
using Avalonia.Platform;

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

    private void UpdateWindowSizeTextBlock()
    {
        WindowSizeTextBlock.Text = $"Window Size: {windowSizeX}x{windowSizeY}";
    }
}