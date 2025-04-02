namespace codecrafters_redis.src
{
    public class PingCommand : IRedisCommand
    {
        public string Execute(Dictionary<string, Value> dataStore, string[] commandParts)
        {
            return "+PONG\r\n";
        }
    }
}
