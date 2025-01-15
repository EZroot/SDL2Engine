public enum DataType
{
    Message = 1,
    Stream = 2
}

public class ProtocolMessage
{
    public DataType Type { get; set; }
    public int Length { get; set; }
    public byte[] Payload { get; set; }

    public byte[] ToBytes()
    {
        var typeBytes = BitConverter.GetBytes((int)Type);
        var lengthBytes = BitConverter.GetBytes(Length);
        return typeBytes.Concat(lengthBytes).Concat(Payload).ToArray();
    }

    public static ProtocolMessage FromBytes(byte[] data)
    {
        var type = (DataType)BitConverter.ToInt32(data, 0);
        var length = BitConverter.ToInt32(data, 4);
        var payload = data.Skip(8).Take(length).ToArray();
        return new ProtocolMessage { Type = type, Length = length, Payload = payload };
    }
}