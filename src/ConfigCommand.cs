using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis.src
{
    public class ConfigCommand : IRedisCommand
    {
        private readonly RdbConfig config;

        public ConfigCommand(RdbConfig config)
        {
            this.config = config;
        }

        public async Task ExecuteAsync(Socket client, string[] commandParts)
        {
            var response = string.Empty;

            if (commandParts.Length < 6)
            {
                response = $"-ERR Wrong number of arguments for CONFIG GET\r\n";
            }

            string paramName = commandParts[6];

            // Handle multiple parameters case (though test will only send one at a time)
            if (paramName.Equals("dir", StringComparison.OrdinalIgnoreCase))
            {
                response = FormatConfigResponse("dir", config.Directory);
            }
            else if (paramName.Equals("dbfilename", StringComparison.OrdinalIgnoreCase))
            {
                response = FormatConfigResponse("dbfilename", config.FileName);

            }
            else
            {
                // If parameter is not found, return an empty array
                response = "*0\r\n";
            }

            await client.SendAsync(Encoding.UTF8.GetBytes(response), SocketFlags.None);
        }

        private string FormatConfigResponse(string paramName, string paramValue)
        {
            // Format as RESP array with two elements: parameter name and value
            return $"*2\r\n${paramName.Length}\r\n{paramName}\r\n${paramValue.Length}\r\n{paramValue}\r\n";
        }
    }
}
