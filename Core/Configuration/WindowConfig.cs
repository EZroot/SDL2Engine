// File: Source/Engine/Core/Configuration/WindowConfig.cs
using System.Diagnostics;
using SDL2Engine.Core.Configuration.Components;

namespace SDL2Engine.Core.Configuration
{
    internal sealed class WindowConfig : IServiceWindowConfig
    {
        private const string DefaultFileName = "winConfig.ini";
        private const string DefaultFolderName = "windowconfig";
        private const string DefaultConfig = "windowname=SDLEngine\nwidth=800\nheight=600\nfullscreen=false";
        private readonly string configDirectory;
        private readonly string configFilePath;
        private readonly Dictionary<string, string> settings = new Dictionary<string, string>();

        public WindowSettings Settings { get; private set; }

        // Constructor
        public WindowConfig()
        {
            configDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultFolderName);
            configFilePath = Path.Combine(configDirectory, DefaultFileName);

            if (!Directory.Exists(configDirectory))
                Directory.CreateDirectory(configDirectory);

            if (!File.Exists(configFilePath))
            {
                File.WriteAllText(configFilePath, DefaultConfig);
                LoadDefaults();
            }
            else
            {
                LoadFromFile();
            }

            ApplySettings();
        }

        private void LoadDefaults()
        {
            foreach (var line in DefaultConfig.Split('\n'))
            {
                var parts = line.Split('=');
                if (parts.Length == 2)
                    settings[parts[0].Trim()] = parts[1].Trim();
            }
        }

        private void LoadFromFile()
        {
            foreach (var line in File.ReadAllLines(configFilePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split('=');
                if (parts.Length == 2)
                    settings[parts[0].Trim()] = parts[1].Trim();
            }
        }

        private void ApplySettings()
        {
            Settings = new WindowSettings
            {
                WindowName = settings.GetValueOrDefault("windowname", "SDL Engine"),
                Width = int.TryParse(settings.GetValueOrDefault("width", "800"), out var width) ? width : 800,
                Height = int.TryParse(settings.GetValueOrDefault("height", "600"), out var height) ? height : 600,
                Fullscreen = bool.TryParse(settings.GetValueOrDefault("fullscreen", "false"), out var fullscreen) && fullscreen
            };

            Utils.Debug.Log($"<color=magenta>Applied Windows Setting:></color>{Settings.WindowName} {Settings.Width}x{Settings.Height} Fullscreen: {Settings.Fullscreen}");
        }

        public void Save()
        {
            using (var writer = new StreamWriter(configFilePath))
            {
                writer.WriteLine($"windowname={Settings.WindowName}");
                writer.WriteLine($"width={Settings.Width}");
                writer.WriteLine($"height={Settings.Height}");
                writer.WriteLine($"fullscreen={Settings.Fullscreen}");
            }
        }
    }
}
