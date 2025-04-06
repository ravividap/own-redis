using System.Net.Sockets;
using System.Net;
using System.Text;
using System;

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
            this.masterPort = !string.IsNullOrEmpty(masterPort) ? Convert.ToInt32(masterPort) : 0;

        }

        public async Task StartAsync()
        {
            if (isRunning)
                return;

            isRunning = true;
            server.Start();

            if (isSlave)
            {
                Task.Run(() => PingMaster(masterHost, masterPort));
            }

            if (!isSlave)
                Console.WriteLine("Master running");

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

        private async Task PingMaster(string masterHost, int masterPort)
        {
            TcpClient server = new(masterHost, masterPort);
            NetworkStream stream = server.GetStream();
            string request = "*1\r\n$4\r\nping\r\n";
            byte[] data = Encoding.ASCII.GetBytes(request);
            stream.Write(data, 0, data.Length);

            data = new Byte[bufferSize];
            var bytesRead = stream.Read(data, 0, data.Length);
            var responseData = Encoding.ASCII.GetString(data, 0, bytesRead);
            Console.WriteLine($"Response: {responseData}");

            request = "*3\r\n$8\r\nREPLCONF\r\n$14\r\nlistening-port\r\n$4\r\n6380\r\n";
            data = Encoding.ASCII.GetBytes(request);
            stream.Write(data, 0, data.Length);

            bytesRead = stream.Read(data, 0, data.Length);
            responseData = Encoding.ASCII.GetString(data, 0, bytesRead);
            Console.WriteLine($"Response: {responseData}");

            request = "*3\r\n$8\r\nREPLCONF\r\n$4\r\ncapa\r\n$6\r\npsync2\r\n";
            data = Encoding.ASCII.GetBytes(request);
            stream.Write(data, 0, data.Length);

            bytesRead = stream.Read(data, 0, data.Length);
            responseData = Encoding.ASCII.GetString(data, 0, bytesRead);
            Console.WriteLine($"Response: {responseData}");

            request = "*3\r\n$5\r\nPSYNC\r\n$1\r\n?\r\n$2\r\n-1\r\n";
            data = Encoding.ASCII.GetBytes(request);
            stream.Write(data, 0, data.Length);

            bytesRead = stream.Read(data, 0, data.Length);
            responseData = Encoding.ASCII.GetString(data, 0, bytesRead);
            Console.WriteLine($"Response: {responseData}");

            while (server.Connected)
            {
                bytesRead = stream.Read(data);

                if (bytesRead <= 0)
                    break;

                responseData = Encoding.ASCII.GetString(data, 0, bytesRead).Trim();

                Console.WriteLine($"slave recived : {responseData}");

                _ = commandProcessor.ProcessCommand(server.Client, responseData);

            }
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
                    await commandProcessor.ProcessCommand(clientSocket, receivedMessage);
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
