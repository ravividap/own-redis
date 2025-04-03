﻿namespace codecrafters_redis.src
{
    public class CommandFactory
    {
        private readonly Dictionary<string, IRedisCommand> _commands;

        public CommandFactory(IDataStore dataStore, RdbConfig config)
        {
            _commands = new Dictionary<string, IRedisCommand>(StringComparer.OrdinalIgnoreCase)
            {
                { "ECHO", new EchoCommand() },
                { "PING", new PingCommand() },
                { "SET", new SetCommand(dataStore) },
                { "GET", new GetCommand(dataStore) },
                { "CONFIG", new ConfigCommand(config)},
                { "KEYS", new KeysCommand(dataStore) }
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
