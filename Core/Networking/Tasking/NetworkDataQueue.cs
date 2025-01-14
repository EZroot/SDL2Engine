using SDL2Engine.Core.Networking.NetData;
using System.Collections.Concurrent;
namespace SDL2Engine.Core.Networking.Tasking;

public class NetworkDataQueue
{
    private readonly ConcurrentQueue<NetworkMessage> _queue = new ConcurrentQueue<NetworkMessage>();
    public bool IsEmpty => _queue.IsEmpty;
    
    public void Enqueue(NetworkMessage message)
    {
        _queue.Enqueue(message);
    }

    public bool TryDequeue(out NetworkMessage message)
    {
        return _queue.TryDequeue(out message);
    }
}
