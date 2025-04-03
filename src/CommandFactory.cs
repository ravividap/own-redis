namespace codecrafters_redis.src
{
    public class CommandFactory
    {
        private readonly Dictionary<string, IRedisCommand> _commands;

        public CommandFactory()
        {
            _commands = new Dictionary<string, IRedisCommand>(StringComparer.OrdinalIgnoreCase)
            {
                { "ECHO", new EchoCommand() },
                { "PING", new PingCommand() },
                { "SET", new SetCommand() },
                { "GET", new GetCommand() },
                { "CONFIG", new ConfigCommand()}
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
