using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis.src
{
    public class PsyncCommand(string replicationId, int offset) : IRedisCommand
    {
        public async Task ExecuteAsync(Socket client, string[] commandParts)
        {
            await client.SendAsync(Encoding.UTF8.GetBytes($"+FULLRESYNC {replicationId} {offset}\r\n"), SocketFlags.None);

            byte[] rdbFileBytes = System.Convert.FromBase64String("UkVESVMwMDEx+glyZWRpcy12ZXIFNy4yLjD6CnJlZGlzLWJpdHPAQPoFY3RpbWXCbQi8ZfoIdXNlZC1tZW3CsMQQAPoIYW9mLWJhc2XAAP/wbjv+wP9aog==");

            byte[] rdbResynchronizationFileMsg = Encoding.ASCII.GetBytes($"${rdbFileBytes.Length}\r\n")
                .Concat(rdbFileBytes)
                .ToArray();


            await client.SendAsync(rdbResynchronizationFileMsg, SocketFlags.None);
        }
    }
}
