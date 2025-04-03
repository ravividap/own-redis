namespace codecrafters_redis.src
{
    public interface IRedisCommand
    {
        string Execute(string[] commandParts);
    }

}
