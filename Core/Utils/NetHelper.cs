using System.Text;
using SDL2Engine.Core.Networking.NetData;

namespace SDL2Engine.Core.Utils;

public static class NetHelper
{
    /// <summary>
    /// Parses the received byte array into a NetworkMessage object.
    /// </summary>
    /// <param name="data">The received data.</param>
    /// <returns>Parsed NetworkMessage.</returns>
    public static NetworkMessage BytesToString(byte[] data)
    {
        var messageString = Encoding.UTF8.GetString(data);
        return new NetworkMessage { Data = data, Message = messageString };
    }

    public static NetworkMessage StringToBytes(string data)
    {
        var raw = Encoding.UTF8.GetBytes(data);
        return new NetworkMessage { Data = raw, Message = data };
    }
}