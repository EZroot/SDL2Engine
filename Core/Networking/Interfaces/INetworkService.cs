namespace SDL2Engine.Core.Networking.Interfaces;

public interface INetworkService
{
    IServer Server { get; }
    IClient Client { get; }
    
    Task Shutdown();
}