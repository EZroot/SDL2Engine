namespace SDL2Engine.Core.Networking.NetData;

public class ClientData
{
    public readonly int Id;
    public readonly string Name;
    public readonly string Address;

    public ClientData(int id, string name, string address)
    {
        Id = id;
        Name = name;
        Address = address;
    }
}