using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis.src
{
    public class GetCommand : IRedisCommand
    {
        private readonly IDataStore dataStore;
        public GetCommand(IDataStore dataStore)
        {
            this.dataStore = dataStore;
        }

        public async Task ExecuteAsync(Socket client, string[] commandParts)
        {
            string key = commandParts[4];
            var data = dataStore.GetData();

            if (!data.TryGetValue(key, out Value value))
                await client.SendAsync(Encoding.UTF8.GetBytes("$-1\r\n"), SocketFlags.None);


            if (value.Expiry.HasValue && value.Expiry.Value < DateTime.UtcNow)
            {
                data.Remove(key); // Clean up expired key
                await client.SendAsync(Encoding.UTF8.GetBytes("$-1\r\n"), SocketFlags.None);
            }

            await client.SendAsync(Encoding.UTF8.GetBytes($"${value.Data.Length}\r\n{value.Data}\r\n"), SocketFlags.None);
        }
    }
}
