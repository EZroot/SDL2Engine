using SDL2Engine.Core.Networking.Interfaces;
using SDL2Engine.Core.Networking.NetData;
using SDL2Engine.Core.Networking.Tasking;
using SDL2Engine.Core.Utils;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SDL2Engine.Events;
using SDL2Engine.Events.EventData;

namespace SDL2Engine.Core.Networking
{
    public class Server : IServer
    {
        private TcpListener _tcpListener;
        private bool _isRunning;
        private readonly ConcurrentDictionary<TcpClient, ClientHandler> _clients;

        public Server()
        {
            _clients = new ConcurrentDictionary<TcpClient, ClientHandler>();
        }

        /// <summary>
        /// Starts the server and begins listening for incoming connections.
        /// </summary>
        /// <param name="port">Port to listen on.</param>
        public async Task Start(int port)
        {
            try
            {
                EventHub.Raise(this, new OnServerStatusChanged(ServerStatus.Starting));
                _tcpListener = new TcpListener(IPAddress.Any, port);
                _tcpListener.Start();
                _isRunning = true;
                EventHub.Raise(this, new OnServerStatusChanged(ServerStatus.Started));
                while (_isRunning)
                {
                    TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    var clientData = new ClientData(0, tcpClient.Client.RemoteEndPoint.ToString(), tcpClient.Client.RemoteEndPoint.AddressFamily.ToString());
                    var clientHandler = new ClientHandler(tcpClient, this);
                    EventHub.Raise(this, new OnServerClientConnectionStatus(clientData, ClientStatus.Connected));

                    if (_clients.TryAdd(tcpClient, clientHandler))
                    {
                        _ = clientHandler.HandleClientAsync(RemoveClient);
                    }
                    else
                    {
                        EventHub.Raise(this, new OnServerClientConnectionStatus(clientData, ClientStatus.TimedOut));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error starting server: {ex.Message}");
                Stop(); // Ensure server is stopped on error
                throw;
            }
        }

        /// <summary>
        /// Stops the server and disconnects all connected clients.
        /// </summary>
        public async Task Stop()
        {
            try
            {
                _isRunning = false;
                _tcpListener?.Stop();
                Debug.Log("Server has stopped listening for new connections.");

                foreach (var kvp in _clients)
                {
                    await kvp.Value.DisconnectAsync();
                }

                _clients.Clear();
                Debug.Log("All clients have been disconnected.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error stopping server: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Removes a client from the active clients list.
        /// </summary>
        /// <param name="tcpClient">The client to remove.</param>
        private void RemoveClient(TcpClient tcpClient)
        {
            if (_clients.TryRemove(tcpClient, out var handler))
            {
                var clientData = new ClientData(0, tcpClient.Client.RemoteEndPoint.ToString(), tcpClient.Client.RemoteEndPoint.AddressFamily.ToString());
                EventHub.Raise(this, new OnServerClientConnectionStatus(clientData, ClientStatus.Disconnected));
                Debug.Log($"Client {tcpClient.Client.RemoteEndPoint} has been removed.");
            }
            else
            {
                Debug.LogError("Failed to remove client from the client list.");
            }
        }
        
        /// <summary>
        /// Broadcasts a message to all connected clients.
        /// </summary>
        /// <param name="message">The message to broadcast.</param>
        public async Task BroadcastMessageAsync(string message)
        {
            var clientsList = _clients.Values.ToList();

            for (int i = 0; i < clientsList.Count; i++)
            {
                byte[] data = NetHelper.StringToBytes($"{message}").Data;
                var client = clientsList[i];
                await client.SendDataAsync(data);
            }

            Debug.Log($"Broadcasted message to {_clients.Count} clients: {message}");
        }


        /// <summary>
        /// Inner class to handle individual client communication.
        /// </summary>
        private class ClientHandler
        {
            private readonly TcpClient _tcpClient;
            private readonly Server _server;
            private NetworkStream _networkStream;
            private CancellationTokenSource _cancellationTokenSource;

            public ClientHandler(TcpClient tcpClient, Server server)
            {
                _server = server;
                _tcpClient = tcpClient;
                _networkStream = _tcpClient.GetStream();
                _cancellationTokenSource = new CancellationTokenSource();
            }

            /// <summary>
            /// Handles communication with the connected client.
            /// </summary>
            /// <param name="onDisconnect">Callback to invoke on disconnection.</param>
            public async Task HandleClientAsync(Action<TcpClient> onDisconnect)
            {
                byte[] buffer = new byte[1024];

                try
                {
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);

                        if (bytesRead == 0)
                        {
                            var clientData = new ClientData(0, _tcpClient.Client.RemoteEndPoint.ToString(), _tcpClient.Client.RemoteEndPoint.AddressFamily.ToString());
                            EventHub.Raise(this, new OnServerClientConnectionStatus(clientData, ClientStatus.Disconnected));
                            break;
                        }

                        byte[] receivedData = new byte[bytesRead];
                        Array.Copy(buffer, receivedData, bytesRead);

                        EventHub.Raise(this, new OnServerMessageRecieved(new RawByteData(receivedData)));
                        
                        var messageBytes = NetHelper.BytesToString(receivedData);
                        // var whisperMsg = NetHelper.StringToBytes("Server Whisper:>"+messageBytes.Message);
                        // await SendDataAsync(whisperMsg.Data);
                        await _server.BroadcastMessageAsync($"{messageBytes.Message}");
                    }
                }
                catch (OperationCanceledException)
                {
                    Debug.Log($"Handling of client {_tcpClient.Client.RemoteEndPoint} has been canceled.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error handling client {_tcpClient.Client.RemoteEndPoint}: {ex.Message}");
                }
                finally
                {
                    await DisconnectAsync();
                    onDisconnect?.Invoke(_tcpClient);
                    var clientData = new ClientData(0, _tcpClient.Client.RemoteEndPoint.ToString(), _tcpClient.Client.RemoteEndPoint.AddressFamily.ToString());
                    EventHub.Raise(this, new OnServerClientConnectionStatus(clientData, ClientStatus.Disconnected));
                }
            }

            /// <summary>
            /// Sends data to the client.
            /// </summary>
            /// <param name="data">Data to send.</param>
            public async Task SendDataAsync(byte[] data)
            {
                if (_networkStream == null)
                {
                    Debug.LogError("Cannot send data. Network stream is null.");
                    return;
                }

                try
                {
                    await _networkStream.WriteAsync(data, 0, data.Length);
                    await _networkStream.FlushAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error sending data to {_tcpClient.Client.RemoteEndPoint}: {ex.Message}");
                }
            }

            /// <summary>
            /// Disconnects the client gracefully.
            /// </summary>
            public async Task DisconnectAsync()
            {
                try
                {
                    _cancellationTokenSource.Cancel();

                    if (_networkStream != null)
                    {
                        await _networkStream.FlushAsync();
                        _networkStream.Close();
                    }

                    _tcpClient.Close();
                    Debug.Log($"Disconnected client {_tcpClient.Client.RemoteEndPoint}.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error disconnecting client {_tcpClient.Client.RemoteEndPoint}: {ex.Message}");
                }
            }
        }
    }
}
