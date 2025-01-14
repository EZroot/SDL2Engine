using SDL2Engine.Core.Networking.NetData;

namespace SDL2Engine.Core.Networking.Interfaces;

public interface IServer
{
    public List<ClientConnectionData> Connections { get; }
    Task Start(int port);
    Task Stop();
}