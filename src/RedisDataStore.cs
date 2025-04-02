namespace codecrafters_redis.src
{
    // Repository Pattern for data access
    public class RedisDataStore
    {
        private readonly Dictionary<string, Value> _data = new Dictionary<string, Value>();

        public Dictionary<string, Value> GetData()
        {
            return _data;
        }
    }
}
