namespace codecrafters_redis.src
{
    public class CommandFactory
    {
        private readonly Dictionary<string, IRedisCommand> _commands;

        public CommandFactory(IDataStore dataStore, RdbConfig config, bool isSlave)
        {
            _commands = new Dictionary<string, IRedisCommand>(StringComparer.OrdinalIgnoreCase)
            {
                { "ECHO", new EchoCommand() },
                { "PING", new PingCommand() },
                { "SET", new SetCommand(dataStore) },
                { "GET", new GetCommand(dataStore) },
                { "CONFIG", new ConfigCommand(config)},
                { "KEYS", new KeysCommand(dataStore) },
                { "INFO", new InfoCommand(isSlave)},
                { "REPLCONF", new ReplConfigCommand()},
                { "PSYNC", new PsyncCommand("8371b4fb1155b71f4a04d3e1bc3e18c4a990aeeb", 0)}
            };
        }

        public IRedisCommand GetCommand(string commandName)
        {
            if (_commands.TryGetValue(commandName, out IRedisCommand command))
            {
                return command;
            }

            return null;
        }
    }
}
