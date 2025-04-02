using System.Net;
using System.Net.Sockets;
using System.Text;

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
    Dictionary<string, Value> data = new Dictionary<string, Value>();

    try
    {
        while (client.Connected)
        {
            var bytesRead = await client.ReceiveAsync(buffer); // Read from the client socket


            if (bytesRead > 0)
            {
                var receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                string response = ParseEchoCommand(receivedMessage, data);

                await client.SendAsync(Encoding.UTF8.GetBytes(response), SocketFlags.None);

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

string ParseEchoCommand(string message, Dictionary<string, Value> data)
{
    if (string.IsNullOrWhiteSpace(message))
        return "-ERR Invalid Command\r\n";

    // Splitting based on Redis RESP line endings
    var parts = message.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

    if (parts.Length > 2)
    {
        string commandName = parts[2];

        if (commandName.Equals("ECHO", StringComparison.OrdinalIgnoreCase) && parts[3].StartsWith("$"))
        {
            // Extracting the message
            string echoMessage = parts[4];
            return $"${echoMessage.Length}\r\n{echoMessage}\r\n"; // Bulk string response
        }
        else if (commandName.Equals("PING", StringComparison.OrdinalIgnoreCase))
        {
            return "+PONG\r\n"; // Simple string response
        }
        else if (commandName.Equals("SET", StringComparison.OrdinalIgnoreCase))
        {
            if (parts.Length > 8 && parts[8].Equals("px", StringComparison.OrdinalIgnoreCase))
            {
                int expiryMilliSeconds = int.Parse(parts[10]);
                data.Add(parts[4], new Value { Data = parts[6], Expiry = DateTime.UtcNow.AddMilliseconds(expiryMilliSeconds) });
            }
            else
            {
                data.Add(parts[4], new Value { Data = parts[6] });
            }

            return $"+OK\r\n";
        }
        else if (commandName.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            var key = parts[4];
            if (!data.ContainsKey(key))
                return $"$-1\r\n";

            var value = data[key];
            if (value.Expiry < DateTime.UtcNow)
                return $"$-1\r\n";

            return $"${value.Data.Length}\r\n{value.Data}\r\n";
        }
    }

    return "-ERR Invalid Command\r\n"; // Redis-style error response
}

public class Value
{
    public string? Data { get; set; }

    public DateTime? Expiry { get; set; }
}