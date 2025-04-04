using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis.src
{
    public class RedisCommandProcessor
    {
        private readonly CommandFactory _commandFactory;
        public RedisCommandProcessor(RdbConfig config, bool isSlave)
        {
            var dataStore = new RedisDataStore(config);
            _commandFactory = new CommandFactory(dataStore, config, isSlave);
        }

        public async Task ProcessCommand(Socket client, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                await client.SendAsync(Encoding.UTF8.GetBytes("-ERR Invalid Command\r\n"), SocketFlags.None);
            }

            // Splitting based on Redis RESP line endings
            string[] parts = message.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 2)
            {
                string commandName = parts[2];
                IRedisCommand command = _commandFactory.GetCommand(commandName);

                if (command != null)
                {
                    try
                    {
                        await command.ExecuteAsync(client, parts);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Command execution error: {ex.Message}");
                        await client.SendAsync(Encoding.UTF8.GetBytes($"-ERR {ex.Message}\r\n"), SocketFlags.None);
                    }
                }
            }

            await client.SendAsync(Encoding.UTF8.GetBytes("-ERR Invalid Command\r\n"), SocketFlags.None);
        }
    }
}
