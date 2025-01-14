using SDL2Engine.Core.Networking.Interfaces;
using SDL2Engine.Core.Networking.NetData;
using SDL2Engine.Core.Networking.Tasking;
using SDL2Engine.Core.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;
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
        private int _clientCount;
        private TcpListener _tcpListener;
        private bool _isRunning;
        private readonly ConcurrentDictionary<int, ClientHandler> _clients;

        public List<ClientConnectionData> Connections => _clients.Values.Select(handler => handler.ClientConnectionData).ToList();
        public bool IsServer => _isRunning;

        public Server()
        {
            _clients = new ConcurrentDictionary<int, ClientHandler>();
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
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    var clientConnectionData = new ClientConnectionData(_clientCount, tcpClient);
                    var clientData = new ClientData(clientConnectionData.Id, clientConnectionData.TcpClient.Client.RemoteEndPoint.ToString(), clientConnectionData.TcpClient.Client.RemoteEndPoint.AddressFamily.ToString());
                    var clientHandler = new ClientHandler(clientConnectionData, this);
                    EventHub.Raise(this, new OnServerClientConnectionStatus(clientData, ClientStatus.Connected));

                    if (_clients.TryAdd(clientConnectionData.Id, clientHandler))
                    {
                        _ = clientHandler.HandleClientAsync(RemoveClient);
                    }
                    else
                    {
                        EventHub.Raise(this, new OnServerClientConnectionStatus(clientData, ClientStatus.TimedOut));
                    }

                    _clientCount++;
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
        /// <param name="clientConnectionData">The client to remove.</param>
        private void RemoveClient(ClientConnectionData clientConnectionData)
        {
            if (_clients.TryRemove(clientConnectionData.Id, out var handler))
            {
                var clientData = new ClientData(clientConnectionData.Id, clientConnectionData.TcpClient.Client.RemoteEndPoint.ToString(), clientConnectionData.TcpClient.Client.RemoteEndPoint.AddressFamily.ToString());
                EventHub.Raise(this, new OnServerClientConnectionStatus(clientData, ClientStatus.Disconnected));
                Debug.Log($"Client {clientConnectionData.TcpClient.Client.RemoteEndPoint} has been removed from client connections.");
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
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var protocolMessage = new ProtocolMessage
            {
                Type = DataType.Message,
                Length = messageBytes.Length,
                Payload = messageBytes
            };

            var dataToSend = protocolMessage.ToBytes();
            var clientsList = _clients.Values.ToList();

            foreach (var clientHandler in clientsList)
            {
                try
                {
                    await clientHandler.SendDataAsync(dataToSend);
                }
                catch (Exception ex)
                {
                    var clientConnectionData = clientHandler.ClientConnectionData;
                    Debug.LogError($"Error sending message to client {clientConnectionData.Id} {clientConnectionData.TcpClient.Client.RemoteEndPoint}: {ex.Message}");
                }
            }

            Debug.Log($"Broadcasted message to ({_clients.Count}) clients: <color=blue>{message}</color>");
        }

        public async Task BroadcastStreamDataAsync(byte[] data)
        {
            var protocolMessage = new ProtocolMessage
            {
                Type = DataType.Stream,
                Length = data.Length,
                Payload = data
            };

            var messageBytes = protocolMessage.ToBytes();
            var clientsList = _clients.Values.ToList();

            foreach (var clientHandler in clientsList)
            {
                try
                {
                    await clientHandler.SendDataAsync(messageBytes);
                }
                catch (Exception ex)
                {
                    var clientConnectionData = clientHandler.ClientConnectionData;
                    Debug.LogError($"Error sending stream data to client {clientConnectionData.Id} {clientConnectionData.TcpClient.Client.RemoteEndPoint}: {ex.Message}");
                }
            }

            Debug.Log($"Broadcasted stream data to ({_clients.Count}) clients.");
        }

        /// <summary>
        /// Inner class to handle individual client communication.
        /// </summary>
        private class ClientHandler
        {
            public ClientConnectionData ClientConnectionData { get; }
            private readonly Server _server;
            private NetworkStream _networkStream;
            private CancellationTokenSource _cancellationTokenSource;

            public ClientHandler(ClientConnectionData clientConnection, Server server)
            {
                _server = server;
                ClientConnectionData = clientConnection;
                _networkStream = ClientConnectionData.TcpClient.GetStream();
                _cancellationTokenSource = new CancellationTokenSource();
            }

            /// <summary>
            /// Handles communication with the connected client.
            /// </summary>
            /// <param name="onDisconnect">Callback to invoke on disconnection.</param>
            public async Task HandleClientAsync(Action<ClientConnectionData> onDisconnect)
            {
                byte[] buffer = new byte[4096];

                try
                {
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        // Read the header first (8 bytes: 4 for type, 4 for length)
                        int headerBytesRead = 0;
                        while (headerBytesRead < 8)
                        {
                            int read = await _networkStream.ReadAsync(buffer, headerBytesRead, 8 - headerBytesRead, _cancellationTokenSource.Token);
                            if (read == 0)
                                throw new Exception("Client disconnected");
                            headerBytesRead += read;
                        }
                        var header = ProtocolMessage.FromBytes(buffer.Take(8).ToArray());
                        DataType dataType = header.Type;
                        int payloadLength = header.Length;

                        byte[] payload = new byte[payloadLength];
                        int payloadBytesRead = 0;
                        while (payloadBytesRead < payloadLength)
                        {
                            int read = await _networkStream.ReadAsync(payload, payloadBytesRead, payloadLength - payloadBytesRead, _cancellationTokenSource.Token);
                            if (read == 0)
                                throw new Exception("Client disconnected during payload reception");
                            payloadBytesRead += read;
                        }

                        var message = "";
                        switch (dataType)
                        {
                            case DataType.None:
                                Debug.Log("<color=yellow>NO PROTOCOL SET FOR RECIEVING MESSAGE!</color> Defaulting to DataType.Message");
                                message = Encoding.UTF8.GetString(payload);
                                EventHub.Raise(this, new OnServerMessageRecieved(new RawByteData(payload)));
                                await _server.BroadcastMessageAsync(message);
                                break;
                            
                            case DataType.Message:
                                message = Encoding.UTF8.GetString(payload);
                                EventHub.Raise(this, new OnServerMessageRecieved(new RawByteData(payload)));
                                await _server.BroadcastMessageAsync(message);
                                break;

                            case DataType.Stream:
                                await ProcessStreamDataAsync(payload);
                                break;

                            default:
                                Debug.LogError("Unknown data type received.");
                                break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Debug.Log($"Handling of client {ClientConnectionData.TcpClient.Client.RemoteEndPoint} has been canceled.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error handling client {ClientConnectionData.TcpClient.Client.RemoteEndPoint}: {ex.Message}");
                }
                finally
                {
                    var clientData = new ClientData(ClientConnectionData.Id, ClientConnectionData.TcpClient.Client.RemoteEndPoint.ToString(), ClientConnectionData.TcpClient.Client.RemoteEndPoint.AddressFamily.ToString());
                    EventHub.Raise(this, new OnServerClientConnectionStatus(clientData, ClientStatus.Disconnected));
                    onDisconnect?.Invoke(ClientConnectionData);
                    await DisconnectAsync();
                }
            }
            
            private async Task ProcessStreamDataAsync(byte[] data)
            {
                try
                {
                    EventHub.Raise(this, new OnServerStreamDataReceived(new RawByteData(data)));
                    await _server.BroadcastStreamDataAsync(data);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing stream data: {ex.Message}");
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
                    Debug.LogError($"Error sending data to {ClientConnectionData.TcpClient.Client.RemoteEndPoint}: {ex.Message}");
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

                    ClientConnectionData.TcpClient.Close();
                    Debug.Log($"Disconnected client {ClientConnectionData.TcpClient.Client.RemoteEndPoint}.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error disconnecting client {ClientConnectionData.TcpClient.Client.RemoteEndPoint}: {ex.Message}");
                }
            }
        }
    }
}
