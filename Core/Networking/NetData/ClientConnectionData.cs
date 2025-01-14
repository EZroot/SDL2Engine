using System.Net.Sockets;

namespace SDL2Engine.Core.Networking.NetData;

public class ClientConnectionData
{
    public readonly int Id;
    public readonly TcpClient TcpClient;

    public ClientConnectionData(int id, TcpClient tcpClient)
    {
        Id = id;
        TcpClient = tcpClient;
    }
}