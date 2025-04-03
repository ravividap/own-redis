using System.Net.Sockets;
using System.Net;
using System.Text;

namespace codecrafters_redis.src
{
    public class RedisServer
    {
        private readonly TcpListener _server;
        private readonly RedisCommandProcessor _commandProcessor;
        private readonly int _bufferSize;
        private bool _isRunning;
        private readonly RdbConfig _rdbConfig;

        public RedisServer(int port, RdbConfig config, bool isSlave, int bufferSize = 1024)
        {
            _server = new TcpListener(IPAddress.Any, port);
            _commandProcessor = new RedisCommandProcessor(config, isSlave);
            _bufferSize = bufferSize;
            _isRunning = false;
            _rdbConfig = config;
        }

        public async Task StartAsync()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _server.Start();

            try
            {
                while (_isRunning)
                {
                    Socket clientSocket = await _server.AcceptSocketAsync();
                    // Use Task.Run for CPU-bound operations or to handle many connections
                    _ = HandleClientAsync(clientSocket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
                Stop();
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _server.Stop();
            Console.WriteLine("Server stopped");
        }

        private async Task HandleClientAsync(Socket clientSocket)
        {
            byte[] buffer = new byte[_bufferSize];

            try
            {
                while (clientSocket.Connected)
                {
                    var bytesRead = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);

                    if (bytesRead <= 0)
                        break;

                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    string response = _commandProcessor.ProcessCommand(receivedMessage);

                    await clientSocket.SendAsync(Encoding.UTF8.GetBytes(response), SocketFlags.None);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket error for client : {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client : {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"Client disconnected");
                clientSocket.Close();
            }
        }
    }
}
