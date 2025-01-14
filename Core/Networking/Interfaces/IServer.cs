namespace SDL2Engine.Core.Networking.Interfaces;

public interface IServer
{
    public 
    Task Start(int port);
    Task Stop();
}