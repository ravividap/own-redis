namespace codecrafters_redis.src
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Logs from your program will appear here!");

            const int port = 6379; // Default Redis port
            var server = new RedisServer(port);

            await server.StartAsync();
        }
    }
}
