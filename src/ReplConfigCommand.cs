namespace codecrafters_redis.src
{
    public class ReplConfigCommand : IRedisCommand
    {
        public string Execute(string[] commandParts)
        {
            return "+OK\r\n";
        }
    }
}
