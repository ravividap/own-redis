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
    await HandleClientSocketAsync(client);
}

async Task HandleClientSocketAsync(Socket client)
{
    byte[] buffer = new byte[256];

    try
    {
        while (client.Connected)
        {
            var bytesRead = await client.ReceiveAsync(buffer); // Read from the client socket

            if (bytesRead > 0)
                await client.SendAsync(Encoding.UTF8.GetBytes("+PONG\r\n"), SocketFlags.None);
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



