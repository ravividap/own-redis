using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis.src
{
    public class ReplConfigCommand : IRedisCommand
    {
        public async Task ExecuteAsync(Socket client, string[] commandParts)
        {
            string getack = commandParts[4];

            if (getack.Equals("GETACK", StringComparison.OrdinalIgnoreCase))
            {
                await client.SendAsync(Encoding.UTF8.GetBytes("*3\r\n$8\r\nreplconf\r\n$3\r\nack\r\n$1\r\n0\r\n"), SocketFlags.None);
            }
            else
            {
                await client.SendAsync(Encoding.UTF8.GetBytes("+OK\r\n"), SocketFlags.None);
            }
        }
    }
}
