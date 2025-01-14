namespace SDL2Engine.Events.EventData;

public class OnServerStatusChanged : EventArgs
{
    public readonly ServerStatus ServerStatus;

    public OnServerStatusChanged(ServerStatus serverStatus)
    {
        ServerStatus = serverStatus;
    }
}

public enum ServerStatus
{
    Starting,
    Started,
    Ending,
    Ended
}