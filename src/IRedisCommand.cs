namespace codecrafters_redis.src
{
    public interface IRedisCommand
    {
        string Execute(Dictionary<string, Value> dataStore, string[] commandParts, RdbConfig config);
    }

}
