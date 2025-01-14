using SDL2Engine.Core.Networking.NetData;

namespace SDL2Engine.Events.EventData;

public class OnServerMessageRecieved : EventArgs
{
    public RawByteData Data;

    public OnServerMessageRecieved(RawByteData data)
    {
        Data = data;
    }
}