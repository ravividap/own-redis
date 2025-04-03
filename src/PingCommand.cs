namespace codecrafters_redis.src
{
    public class PingCommand : IRedisCommand
    {
        public string Execute(Dictionary<string, Value> dataStore, string[] commandParts, RdbConfig config)
        {
            return "+PONG\r\n";
        }
    }
}
