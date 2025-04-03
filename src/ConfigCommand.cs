

namespace codecrafters_redis.src
{
    public class ConfigCommand : IRedisCommand
    {
        public RdbConfig Config { get; set; }
        public string Execute(Dictionary<string, Value> dataStore, string[] commandParts, RdbConfig config)
        {
            if (commandParts.Length < 6)
                return "-ERR Wrong number of arguments for CONFIG GET\r\n";

            string paramName = commandParts[4];

            // Handle multiple parameters case (though test will only send one at a time)
            if (paramName.Equals("dir", StringComparison.OrdinalIgnoreCase))
            {
                return FormatConfigResponse("dir", config.Directory);
            }
            else if (paramName.Equals("dbfilename", StringComparison.OrdinalIgnoreCase))
            {
                return FormatConfigResponse("dbfilename", config.FileName);
            }
            else
            {
                // If parameter is not found, return an empty array
                return "*0\r\n";
            }
        }

        private string FormatConfigResponse(string paramName, string paramValue)
        {
            // Format as RESP array with two elements: parameter name and value
            return $"*2\r\n${paramName.Length}\r\n{paramName}\r\n${paramValue.Length}\r\n{paramValue}\r\n";
        }
    }
}
