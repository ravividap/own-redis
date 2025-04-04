using System.Net.Sockets;

namespace codecrafters_redis.src
{
    public interface IRedisCommand
    {
        Task ExecuteAsync(Socket client, string[] commandParts);
    }

}
