using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis.src
{
    public class SetCommand : IRedisCommand
    {
        private readonly IDataStore dataStore;
        private readonly bool isSlave;

        public SetCommand(IDataStore dataStore, bool isSalve)
        {
            this.dataStore = dataStore;
            this.isSlave = isSalve;
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

            if (!isSlave)
                await client.SendAsync(Encoding.UTF8.GetBytes("+OK\r\n"), SocketFlags.None);

            if (isSlave)
            {
                var command = Encoding.UTF8.GetBytes($"*3\r\n$3\r\nSET\r\n${key.Length}\r\n{key}\r\n${value.Length}\r\n{value}\r\n");
                dataStore.SetOffSet(command.Length);
            }

            var replicas = dataStore.GetReplicas();

            Console.WriteLine($"Number of replicas: {replicas.Count}");

            foreach (var replica in replicas)
            {
                Console.WriteLine($"Master sending set commands, key: {key}, value : {value}");
                var replicaSocket = replica.Value;
                var command = $"*3\r\n$3\r\nSET\r\n${key.Length}\r\n{key}\r\n${value.Length}\r\n{value}\r\n";

                await replicaSocket.SendAsync(Encoding.UTF8.GetBytes(command));
            }
        }
    }
}
