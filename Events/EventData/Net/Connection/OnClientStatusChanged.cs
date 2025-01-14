namespace SDL2Engine.Events.EventData;

public class OnClientStatusChanged : EventArgs
{
    public readonly ClientStatus ClientStatus;

    public OnClientStatusChanged(ClientStatus clientStatus)
    {
        ClientStatus = clientStatus;
    }
}

public enum ClientStatus
{
    Connecting,
    Connected,
    Disconnecting,
    Disconnected,
    ServerClosedConnection,
    TimedOut
}