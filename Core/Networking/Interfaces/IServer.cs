namespace SDL2Engine.Core.Networking.Interfaces;

public interface IServer
{
    Task Start(int port);
    Task Stop();
}