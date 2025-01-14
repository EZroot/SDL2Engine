using SDL2Engine.Core.Networking.NetData;

namespace SDL2Engine.Events.EventData;

/// <summary>
/// This is the SERVER's point of view of a clients connection
/// </summary>
public class OnServerClientConnectionStatus: EventArgs
{
    public readonly ClientData ClientData;
    public readonly ClientStatus ClientStatus;

    public OnServerClientConnectionStatus(ClientData clientData, ClientStatus clientStatus)
    {
        ClientData = clientData;
        ClientStatus = clientStatus;
    }
}