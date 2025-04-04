using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis.src
{
    public class PingCommand : IRedisCommand
    {
        public async Task ExecuteAsync(Socket client, string[] commandParts)
        {
            await client.SendAsync(Encoding.UTF8.GetBytes("+PONG\r\n"), SocketFlags.None);
        }
    }
}
