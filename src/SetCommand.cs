using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis.src
{
    public class SetCommand : IRedisCommand
    {
        private readonly IDataStore dataStore;

        public SetCommand(IDataStore dataStore)
        {
            this.dataStore = dataStore;
        }
        public async Task ExecuteAsync(Socket client, string[] commandParts)
        {
            string key = commandParts[4];
            string value = commandParts[6];
            DateTime? expiry = null;

            var data = dataStore.GetData();

            // Check for expiry parameter
            if (commandParts.Length > 8 && commandParts[8].Equals("px", StringComparison.OrdinalIgnoreCase))
            {
                int expiryMilliSeconds = int.Parse(commandParts[10]);
                expiry = DateTime.UtcNow.AddMilliseconds(expiryMilliSeconds);
            }

            // Use TryAdd with update logic to handle existing keys
            if (data.ContainsKey(key))
            {
                data[key] = new Value { Data = value, Expiry = expiry };
            }
            else
            {
                data.Add(key, new Value { Data = value, Expiry = expiry });
            }

            await client.SendAsync(Encoding.UTF8.GetBytes("+OK\r\n"), SocketFlags.None);
        }
    }
}
