using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 6379);
server.Start();
var client = server.AcceptSocket();


byte[] buffer = new byte[256];
int bytesRead = client.Receive(buffer); // Read from the client socket

client.Send(Encoding.UTF8.GetBytes("+PONG\r\n"), SocketFlags.None);

client.Shutdown(SocketShutdown.Both);
client.Close();
server.Stop();

