using SDL2Engine.Core.Networking.Interfaces;

namespace SDL2Engine.Core.Networking;

public class NetworkService : INetworkService
{
    private IClient m_client;
    private IServer m_server;
}