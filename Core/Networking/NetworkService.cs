using SDL2Engine.Core.Networking.Interfaces;
using SDL2Engine.Core.Networking.NetData;
using SDL2Engine.Core.Utils;
using System;
using SDL2Engine.Core.Networking.Tasking;

namespace SDL2Engine.Core.Networking
{
    public class NetworkService : INetworkService
    {
        private IClient _client;
        private IServer _server;
        private readonly NetworkDataQueue _dataQueue;

        public IServer Server => _server;
        public IClient Client => _client;

        public NetworkService()
        {
            _dataQueue = new NetworkDataQueue();
            _client = new Client(_dataQueue);
            _server = new Server();
        }

        /// <summary>
        /// Cleans up network resources.
        /// </summary>
        public async Task Shutdown()
        {
            try
            {
                await _client.Disconnect();
                await _server.Stop();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during network shutdown: {ex.Message}");
            }
        }
    }
}
