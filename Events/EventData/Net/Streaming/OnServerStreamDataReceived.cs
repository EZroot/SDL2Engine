using SDL2Engine.Core.Networking.NetData;

namespace SDL2Engine.Events.EventData;

public class OnServerStreamDataReceived : EventArgs
{
    public RawByteData Data;

    public OnServerStreamDataReceived(RawByteData data)
    {
        Data = data;
    }
}