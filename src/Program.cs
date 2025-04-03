namespace codecrafters_redis.src
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Logs from your program will appear here!");

            const int port = 6379; // Default Redis port
            var config = new RdbConfig();
            if (args.Length > 0)
            {
                if (args[0].Equals("--dir", StringComparison.OrdinalIgnoreCase))
                {
                    config.Directory = args[1];
                }
                if (args[2].Equals("--dbfilename", StringComparison.OrdinalIgnoreCase))
                {
                    config.FileName = args[3];
                }
            }


            var server = new RedisServer(port, config);

            await server.StartAsync();
        }
    }
}
