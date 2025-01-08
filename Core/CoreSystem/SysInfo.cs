namespace SDL2Engine.Core.CoreSystem.Configuration
{
    /// <summary>
    /// Purely for recording information about the system.
    /// Doesn't effect any functionality.
    /// </summary>
    internal sealed class SysInfo : IServiceSysInfo
    {
        private SDLRenderInfo m_sdlRenderInfo;
        public SDLRenderInfo SDLRenderInfo => m_sdlRenderInfo;

        public SysInfo()
        {
            m_sdlRenderInfo = new SDLRenderInfo();
        }

        public void SetInfoCurrentDriver(string currentDriver)
        {
            m_sdlRenderInfo.CurrentRenderDriver = currentDriver;
        }

        public void SetInfoCurrentAvailableDrivers(params string[] availableDrivers)
        {
            m_sdlRenderInfo.AvailableRenderDrivers = availableDrivers;
        }
    }

    public struct SDLRenderInfo
    {
        public string CurrentRenderDriver;
        public string[] AvailableRenderDrivers;
    }
}