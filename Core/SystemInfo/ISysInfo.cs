namespace SDL2Engine.Core.CoreSystem.Configuration
{
    public interface ISysInfo
    {
        public SDLRenderInfo SDLRenderInfo {get;}
        public void SetInfoCurrentDriver(string currentDriver);
        public void SetInfoCurrentAvailableDrivers(params string[] availableDrivers);
    }
}

