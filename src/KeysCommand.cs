using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis.src
{
    public class KeysCommand(IDataStore dataStore) : IRedisCommand
    {
        
        public async Task ExecuteAsync(Socket client, string[] commandParts)
        {
            Console.WriteLine(commandParts[4]);

            if (commandParts[4] == "*")
            {
                await client.SendAsync(Encoding.UTF8.GetBytes(BuildArrayString(dataStore.GetData().Keys.ToArray())), SocketFlags.None);
            }

            await client.SendAsync(Encoding.UTF8.GetBytes("$-1\r\n"), SocketFlags.None);
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
