namespace SDL2Engine.Core.CoreSystem.Configuration.Components
{
    public struct WindowSettings
    {
        public string WindowName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Fullscreen { get; set; }

        public WindowSettings(string windowName, int width, int height, bool fullscreen)
        {
            WindowName = windowName;
            Width = width;
            Height = height;
            Fullscreen = fullscreen;
        }
    }
}
