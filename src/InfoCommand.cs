using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis.src
{
    public class InfoCommand(bool isSlave) : IRedisCommand
    {
        public async Task ExecuteAsync(Socket client, string[] commandParts)
        {
            var role = isSlave ? "slave" : "master";
            var info = new Dictionary<string, string>();

            info.Add("role", role);
            info.Add("master_replid", "8371b4fb1155b71f4a04d3e1bc3e18c4a990aeeb");
            info.Add("master_repl_offset", "0");

            StringBuilder sb = new StringBuilder();

            foreach (var kv in info)
            {
                var kvString = $"{kv.Key}:{kv.Value}";
                sb.AppendLine(kvString);
            }

            await client.SendAsync(Encoding.UTF8.GetBytes(BuildBulkString(sb.ToString())), SocketFlags.None);
        }

        private string BuildBulkString(string value)
        {
            return $"${value.Length}\r\n{value}\r\n";
        }
    }
}
