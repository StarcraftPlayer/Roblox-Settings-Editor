using System;
using System.IO;
using System.Text.Json;

public class AppSettings
{
    public string FilePath { get; set; } = "";
    public int FPS { get; set; } = 60;
    public int GraphicsLevel { get; set; } = 10;
    public int VolumeLevel { get; set; } = 10;
    public bool Fullscreen { get; set; } = false;
    public int windowSizeX { get; set; } = 640;
    public int windowSizeY { get; set; } = 360;

    private static string getSaveFilePath()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string saveDirectory = Path.Combine(appDataPath, "RobloxSettingsModifier");
        Directory.CreateDirectory(saveDirectory);
        return Path.Combine(saveDirectory, "settings.json");
    }

    public bool SaveSettings()
    {
        string saveFilePath = getSaveFilePath();
        try
        {
            string jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(saveFilePath, jsonString);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings: {ex.Message}");
            return false;
        }
    }

    public bool LoadSettings()
    {
        string saveFilePath = getSaveFilePath();
        if (!File.Exists(saveFilePath))
        {
            return false;
        }

        try
        {
            string jsonString = File.ReadAllText(saveFilePath);
            var loadedSettings = JsonSerializer.Deserialize<AppSettings>(jsonString);
            if (loadedSettings != null)
            {
                this.FilePath = loadedSettings.FilePath;
                this.FPS = loadedSettings.FPS;
                this.GraphicsLevel = loadedSettings.GraphicsLevel;
                this.VolumeLevel = loadedSettings.VolumeLevel;
                this.Fullscreen = loadedSettings.Fullscreen;
                this.windowSizeX = loadedSettings.windowSizeX;
                this.windowSizeY = loadedSettings.windowSizeY;
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings: {ex.Message}");
        }
        return false;
    }
}