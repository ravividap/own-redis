namespace codecrafters_redis.src
{
    public class PingCommand : IRedisCommand
    {
        public string Execute(string[] commandParts)
        {
            return "+PONG\r\n";
        }
    }
}
