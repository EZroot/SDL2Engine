// File: Source/Engine/Core/Configuration/WindowSettings.cs
namespace SDL2Engine.Core.Configuration.Components
{
    public struct WindowSettings
    {
        public string WindowName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Fullscreen { get; set; }
    }
}
