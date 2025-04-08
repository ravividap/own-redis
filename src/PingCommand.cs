using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis.src
{
    public class PingCommand(IDataStore dataStore, bool isSlave) : IRedisCommand
    {
        public async Task ExecuteAsync(Socket client, string[] commandParts)
        {
            dataStore.SetOffSet(14);
            await client.SendAsync(Encoding.UTF8.GetBytes("*1\r\n$4\r\nPONG\r\n"), SocketFlags.None);
            //await client.SendAsync(Encoding.UTF8.GetBytes("+PONG\r\n"), SocketFlags.None);
        }
    }
}
