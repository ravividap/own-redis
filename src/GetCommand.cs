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

            var response = "$-1\r\n";

            if (!data.TryGetValue(key, out Value value))
            {
                response = "$-1\r\n";
            }

            if (value != null)
            {
                if (value.Expiry.HasValue && value.Expiry.Value < DateTime.UtcNow)
                {
                    data.Remove(key); // Clean up expired key
                    response = "$-1\r\n";
                }
                else
                {
                    response = $"${value.Data.Length}\r\n{value.Data}\r\n";
                }
            }
            

            await client.SendAsync(Encoding.UTF8.GetBytes(response), SocketFlags.None);
        }
    }
}
