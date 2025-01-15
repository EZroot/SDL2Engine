namespace SDL2Engine.Core.Networking.Interfaces;

public interface IClient
{
    bool IsConnected { get; }
    Task Connect(string address, int port);
    Task Disconnect();
    Task ReceiveDataAsync(CancellationToken cancellationToken);
    Task SendDataAsync(byte[] data);
}