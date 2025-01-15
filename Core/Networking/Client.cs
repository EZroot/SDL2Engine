using SDL2Engine.Core.Networking.Interfaces;
using SDL2Engine.Core.Networking.NetData;
using SDL2Engine.Core.Networking.Tasking;
using SDL2Engine.Core.Utils;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SDL2Engine.Events;
using SDL2Engine.Events.EventData;

namespace SDL2Engine.Core.Networking
{
    public class Client : IClient
    {
        private readonly NetworkDataQueue _dataQueue;
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isConnected;
        public bool IsConnected => _isConnected;
        public Client(NetworkDataQueue dataQueue)
        {
            _dataQueue = dataQueue;
            EventHub.Subscribe<OnClientStatusChanged>(OnClientStatusChanged);
        }
        
        private void OnClientStatusChanged(object sender, OnClientStatusChanged e)
        {
            
            switch (e.ClientStatus)
            {
                case ClientStatus.Connected:
                    _isConnected = true;
                    break;
                case ClientStatus.Disconnected:
                    _isConnected = false;
                    break;
            }
        }
        
        /// <summary>
        /// Connects to the server at the specified address and port.
        /// </summary>
        /// <param name="address">Server IP address or hostname.</param>
        /// <param name="port">Server port.</param>
        public async Task Connect(string address, int port)
        {
            try
            {
                _tcpClient = new TcpClient();
                EventHub.Raise(this, new OnClientStatusChanged(ClientStatus.Connecting));

                await _tcpClient.ConnectAsync(address, port);
                _networkStream = _tcpClient.GetStream();
                EventHub.Raise(this, new OnClientStatusChanged(ClientStatus.Connected));
                _cancellationTokenSource = new CancellationTokenSource();
                _ = ReceiveDataAsync(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                EventHub.Raise(this, new OnClientStatusChanged(ClientStatus.Disconnected));

                Debug.LogError($"Failed to connect to server: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Disconnects from the server gracefully.
        /// </summary>
        public async Task Disconnect()
        {
            try
            {
                EventHub.Raise(this, new OnClientStatusChanged(ClientStatus.Disconnecting));

                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                }

                if (_networkStream != null)
                {
                    await _networkStream.FlushAsync();
                    _networkStream.Close();
                }

                if (_tcpClient != null)
                {
                    _tcpClient.Close();
                }
                EventHub.Raise(this, new OnClientStatusChanged(ClientStatus.Disconnected));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during disconnection: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Processes incoming stream data from the server.
        /// </summary>
        /// <param name="data">The stream data payload.</param>
        private void ProcessStreamData(byte[] data)
        {
            try
            {
                EventHub.Raise(this, new OnClientStreamDataReceived(new RawByteData(data)));
                // _streamDataQueue.Enqueue(new RawByteData(data));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing stream data: {ex.Message}");
            }
        }
        /// <summary>
        /// Continuously receives data from the server.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to stop receiving.</param>
        public async Task ReceiveDataAsync(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[4096]; 
            int bufferOffset = 0;
            int expectedLength = 0;
            DataType currentDataType = DataType.Message;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = await _networkStream.ReadAsync(buffer, bufferOffset, buffer.Length - bufferOffset,
                        cancellationToken);

                    if (bytesRead == 0)
                    {
                        EventHub.Raise(this, new OnClientStatusChanged(ClientStatus.ServerClosedConnection));
                        await Disconnect();
                        break;
                    }

                    bufferOffset += bytesRead;
                    int processedBytes = 0;

                    while (true)
                    {
                        if (expectedLength == 0)
                        {
                            if (bufferOffset - processedBytes >= 8)
                            {
                                byte[] headerData = buffer.Skip(processedBytes).Take(8).ToArray();
                                var protocolMessage = ProtocolMessage.FromBytes(headerData);
                                currentDataType = protocolMessage.Type;
                                expectedLength = protocolMessage.Length;
                                processedBytes += 8;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (expectedLength > 0)
                        {
                            if (bufferOffset - processedBytes >= expectedLength)
                            {
                                byte[] payloadData = buffer.Skip(processedBytes).Take(expectedLength).ToArray();
                                processedBytes += expectedLength;
                                var message = "";
                                var networkMessage = new NetworkMessage();
                                switch (currentDataType)
                                {
                                    case DataType.None:
                                        Debug.Log("<color=yellow>NO PROTOCOL SET FOR RECIEVING MESSAGE!</color> Defaulting to DataType.Message");
                                        message = Encoding.UTF8.GetString(payloadData);
                                        networkMessage = new NetworkMessage
                                            { Data = payloadData, Message = message };
                                        _dataQueue.Enqueue(networkMessage);
                                        EventHub.Raise(this, new OnClientMessageRecieved(new RawByteData(payloadData)));
                                        break;
                                    
                                    case DataType.Message:
                                        message = Encoding.UTF8.GetString(payloadData);
                                        networkMessage = new NetworkMessage
                                            { Data = payloadData, Message = message };
                                        _dataQueue.Enqueue(networkMessage);
                                        EventHub.Raise(this, new OnClientMessageRecieved(new RawByteData(payloadData)));
                                        break;

                                    case DataType.Stream:
                                        ProcessStreamData(payloadData);
                                        break;

                                    default:
                                        Debug.LogError("Unknown data type received.");
                                        break;
                                }

                                expectedLength = 0;
                                currentDataType = DataType.Message;
                            }
                            else
                            {
                                Debug.LogError("Not enough data for payload!");
                                break;
                            }
                        }
                    }

                    if (processedBytes < bufferOffset)
                    {
                        Array.Copy(buffer, processedBytes, buffer, 0, bufferOffset - processedBytes);
                        bufferOffset -= processedBytes;
                    }
                    else
                    {
                        bufferOffset = 0;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Stopped receiving data.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error receiving data: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends data to the server.
        /// </summary>
        /// <param name="data">The data to send.</param>
        public async Task SendDataAsync(byte[] data)
        {
            if (_networkStream == null)
            {
                Debug.LogError("Cannot send data. Not connected to the server.");
                return;
            }
            try
            {
                await _networkStream.WriteAsync(data, 0, data.Length);
                await _networkStream.FlushAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error sending data: {ex.Message}");
            }
        }
    }
}
