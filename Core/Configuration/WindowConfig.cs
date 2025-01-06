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
        private readonly string m_configDirectory;
        private readonly string m_configFilePath;
        private readonly Dictionary<string, string> m_settings = new Dictionary<string, string>();

        public WindowSettings Settings { get; private set; }

        // Constructor
        public WindowConfig()
        {
            m_configDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultFolderName);
            m_configFilePath = Path.Combine(m_configDirectory, DefaultFileName);

            if (!Directory.Exists(m_configDirectory))
                Directory.CreateDirectory(m_configDirectory);

            if (!File.Exists(m_configFilePath))
            {
                File.WriteAllText(m_configFilePath, DefaultConfig);
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
                    m_settings[parts[0].Trim()] = parts[1].Trim();
            }
        }

        private void LoadFromFile()
        {
            foreach (var line in File.ReadAllLines(m_configFilePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split('=');
                if (parts.Length == 2)
                    m_settings[parts[0].Trim()] = parts[1].Trim();
            }
        }

        private void ApplySettings()
        {
            Settings = new WindowSettings
            {
                WindowName = m_settings.GetValueOrDefault("windowname", "SDL Engine"),
                Width = int.TryParse(m_settings.GetValueOrDefault("width", "800"), out var width) ? width : 800,
                Height = int.TryParse(m_settings.GetValueOrDefault("height", "600"), out var height) ? height : 600,
                Fullscreen = bool.TryParse(m_settings.GetValueOrDefault("fullscreen", "false"), out var fullscreen) && fullscreen
            };

            Utils.Debug.Log($"<color=magenta>Applied Windows Setting:></color>{Settings.WindowName} {Settings.Width}x{Settings.Height} Fullscreen: {Settings.Fullscreen}");
        }

        public void Save()
        {
            using (var writer = new StreamWriter(m_configFilePath))
            {
                writer.WriteLine($"windowname={Settings.WindowName}");
                writer.WriteLine($"width={Settings.Width}");
                writer.WriteLine($"height={Settings.Height}");
                writer.WriteLine($"fullscreen={Settings.Fullscreen}");
            }
        }
    }
}
