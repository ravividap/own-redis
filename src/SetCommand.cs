namespace codecrafters_redis.src
{
    public class SetCommand : IRedisCommand
    {
        public string Execute(Dictionary<string, Value> dataStore, string[] commandParts)
        {
            string key = commandParts[4];
            string value = commandParts[6];
            DateTime? expiry = null;

            // Check for expiry parameter
            if (commandParts.Length > 8 && commandParts[8].Equals("px", StringComparison.OrdinalIgnoreCase))
            {
                int expiryMilliSeconds = int.Parse(commandParts[10]);
                expiry = DateTime.UtcNow.AddMilliseconds(expiryMilliSeconds);
            }

            // Use TryAdd with update logic to handle existing keys
            if (dataStore.ContainsKey(key))
            {
                dataStore[key] = new Value { Data = value, Expiry = expiry };
            }
            else
            {
                dataStore.Add(key, new Value { Data = value, Expiry = expiry });
            }

            return "+OK\r\n";
        }
    }
}
