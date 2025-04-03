namespace codecrafters_redis.src
{
    public class GetCommand : IRedisCommand
    {
        private readonly Dictionary<string, Value> dataStore;
        public GetCommand(Dictionary<string, Value> dataStore)
        {
            this.dataStore = dataStore;   
        }

        public string Execute(string[] commandParts)
        {
            string key = commandParts[4];

            if (!dataStore.TryGetValue(key, out Value value))
                return "$-1\r\n";

            if (value.Expiry.HasValue && value.Expiry.Value < DateTime.UtcNow)
            {
                dataStore.Remove(key); // Clean up expired key
                return "$-1\r\n";
            }

            return $"${value.Data.Length}\r\n{value.Data}\r\n";
        }
    }
}
