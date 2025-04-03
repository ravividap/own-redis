namespace codecrafters_redis.src
{
    public class GetCommand : IRedisCommand
    {
        private readonly IDataStore dataStore;
        public GetCommand(IDataStore dataStore)
        {
            this.dataStore = dataStore;   
        }

        public string Execute(string[] commandParts)
        {
            string key = commandParts[4];
            var data = dataStore.GetData();

            if (!data.TryGetValue(key, out Value value))
                return "$-1\r\n";

            if (value.Expiry.HasValue && value.Expiry.Value < DateTime.UtcNow)
            {
                data.Remove(key); // Clean up expired key
                return "$-1\r\n";
            }

            return $"${value.Data.Length}\r\n{value.Data}\r\n";
        }
    }
}
