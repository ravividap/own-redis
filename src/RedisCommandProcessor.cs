namespace codecrafters_redis.src
{
    public class RedisCommandProcessor
    {
        private readonly CommandFactory _commandFactory;
        public RedisCommandProcessor(RdbConfig config)
        {
            var dataStore = new RedisDataStore().GetData();
            _commandFactory = new CommandFactory(dataStore, config);
        }

        public string ProcessCommand(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "-ERR Invalid Command\r\n";

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
                        return command.Execute(parts);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Command execution error: {ex.Message}");
                        return $"-ERR {ex.Message}\r\n";
                    }
                }
            }

            return "-ERR Invalid Command\r\n";
        }
    }
}
