using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis.src
{
    public class EchoCommand : IRedisCommand
    {
        public async Task ExecuteAsync(Socket client, string[] commandParts)
        {
            string echoMessage = commandParts[4];

            await client.SendAsync(Encoding.UTF8.GetBytes($"${echoMessage.Length}\r\n{echoMessage}\r\n"), SocketFlags.None);
        }
    }
}
