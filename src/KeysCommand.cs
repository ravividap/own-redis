using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis.src
{
    public class KeysCommand(IDataStore dataStore) : IRedisCommand
    {
        
        public async Task ExecuteAsync(Socket client, string[] commandParts)
        {
            var response = string.Empty;

            if (commandParts[4] == "*")
            {
                response = BuildArrayString(dataStore.GetData().Keys.ToArray());
            }
            else
            {
                response = "$-1\r\n";
            }

            await client.SendAsync(Encoding.UTF8.GetBytes(response), SocketFlags.None);

        }

        private string BuildArrayString(string[] args)
        {
            var answer = string.Format("*{0}\r\n", args.Length);
            foreach (var item in args)
            {
                answer += string.Format("${0}\r\n{1}\r\n", item.Length, item);
            }
            return answer;
        }
    }
}
