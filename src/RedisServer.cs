using System.Net.Sockets;
using System.Net;
using System.Text;

namespace codecrafters_redis.src
{
    public class RedisServer
    {
        private readonly TcpListener server;
        private readonly RedisCommandProcessor commandProcessor;
        private readonly int bufferSize;
        private bool isRunning;
        private readonly RdbConfig rdbConfig;
        private bool isSlave;
        private string masterHost;
        private int masterPort;
        public RedisServer(int port, RdbConfig config, bool isSlave, string masterHost, string masterPort, int bufferSize = 1024)
        {
            server = new TcpListener(IPAddress.Any, port);
            commandProcessor = new RedisCommandProcessor(config, isSlave);
            this.bufferSize = bufferSize;
            isRunning = false;
            rdbConfig = config;
            this.isSlave = isSlave;
            this.masterHost = masterHost;
            this.masterPort = Convert.ToInt32(masterPort);
            
        }

        public async Task StartAsync()
        {
            if (isRunning)
                return;

            isRunning = true;
            server.Start();

            if (isSlave)
            {
                PingMaster(masterHost, masterPort);
            }

            try
            {
                while (isRunning)
                {
                    Socket clientSocket = await server.AcceptSocketAsync();
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

        private void PingMaster(string masterHost, int masterPort)
        {
            TcpClient tcpClient = new(masterHost, masterPort);
            NetworkStream stream = tcpClient.GetStream();
            string request = "*1\r\n$4\r\nping\r\n";
            byte[] data = Encoding.ASCII.GetBytes(request);
            stream.Write(data, 0, data.Length);
        }

        public void Stop()
        {
            isRunning = false;
            server.Stop();
            Console.WriteLine("Server stopped");
        }

        private async Task HandleClientAsync(Socket clientSocket)
        {
            byte[] buffer = new byte[bufferSize];

            try
            {
                while (clientSocket.Connected)
                {
                    var bytesRead = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);

                    if (bytesRead <= 0)
                        break;

                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    string response = commandProcessor.ProcessCommand(receivedMessage);

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
