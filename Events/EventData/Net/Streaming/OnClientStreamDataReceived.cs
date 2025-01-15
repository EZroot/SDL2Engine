using SDL2Engine.Core.Networking.NetData;

namespace SDL2Engine.Events.EventData;

public class OnClientStreamDataReceived : EventArgs
{
    public RawByteData Data;

    public OnClientStreamDataReceived(RawByteData data)
    {
        Data = data;
    }
}