using SDL2Engine.Core.Networking.NetData;

namespace SDL2Engine.Events.EventData;

public class OnClientMessageRecieved : EventArgs
{
    public RawByteData Data;

    public OnClientMessageRecieved(RawByteData data)
    {
        Data = data;
    }
}