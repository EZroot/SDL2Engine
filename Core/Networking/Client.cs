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

        public Client(NetworkDataQueue dataQueue)
        {
            _dataQueue = dataQueue;
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
        /// Continuously receives data from the server.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to stop receiving.</param>
        public async Task ReceiveDataAsync(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    if (bytesRead == 0)
                    {
                        EventHub.Raise(this, new OnClientStatusChanged(ClientStatus.ServerClosedConnection));
                        await Disconnect();
                        break;
                    }

                    byte[] receivedData = new byte[bytesRead];
                    Array.Copy(buffer, receivedData, bytesRead);

                    var message = ParseReceivedData(receivedData);
                    _dataQueue.Enqueue(message);

                    Debug.Log("Received and enqueued a new network message.");
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

        /// <summary>
        /// Parses the received byte array into a NetworkMessage object.
        /// </summary>
        /// <param name="data">The received data.</param>
        /// <returns>Parsed NetworkMessage.</returns>
        private NetworkMessage ParseReceivedData(byte[] data)
        {
            // we'll assume the data is a UTF8 string.
            string messageString = Encoding.UTF8.GetString(data);
            return new NetworkMessage { Data = data, Message = messageString };
        }
    }
}
