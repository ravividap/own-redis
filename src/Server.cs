using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 6379);
server.Start();
server.AcceptSocket(); // wait for client

var client = server.AcceptSocket();

client.Send(Encoding.UTF8.GetBytes("+PONG\r\n"), SocketFlags.None);

client.Shutdown(SocketShutdown.Both);
client.Close();
server.Stop(); 