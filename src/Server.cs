using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 6379);
server.Start();

while (true)
{
    var client = await server.AcceptSocketAsync();

    // Handle each client in a separate task
    // _ = Task.Run(() => HandleClientSocketAsync(client)); good for cpu bound work 

    _ = HandleClientSocketAsync(client); // fire and forget
}

async Task HandleClientSocketAsync(Socket client)
{
    byte[] buffer = new byte[1024];

    try
    {
        while (client.Connected)
        {
            var bytesRead = await client.ReceiveAsync(buffer); // Read from the client socket


            if (bytesRead > 0)
            {
                var receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                string response = ParseEchoCommand(receivedMessage);

                await client.SendAsync(Encoding.UTF8.GetBytes("response\r\n"), SocketFlags.None);

            }
            else
                break;
        }
    }
    catch (SocketException ex)
    {
        Console.WriteLine($"Socket error: {ex.Message}");
    }
    finally
    {
        Console.WriteLine($"Client {client.RemoteEndPoint} disconnected.");
        client.Close();
    }

}

string ParseEchoCommand(string message)
{
    try
    {
        string[]? command = JsonSerializer.Deserialize<string[]>(message);
        if (command != null && command.Length >= 2 && command[0].Equals("ECHO", StringComparison.OrdinalIgnoreCase))
        {
            return command[1]; // Return whatever follows "ECHO"
        }
    }
    catch (JsonException) { }

    return "-ERR Invalid Command"; // Redis-style error response
}