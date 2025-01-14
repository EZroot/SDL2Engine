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
    public static NetworkMessage ParseReceivedData(byte[] data)
    {
        // Implement your actual parsing logic here.
        // For demonstration, we'll assume the data is a UTF8 string.
        string messageString = Encoding.UTF8.GetString(data);
        return new NetworkMessage { Data = data, Message = messageString };
    }
}