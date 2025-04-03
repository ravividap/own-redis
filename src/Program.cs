namespace codecrafters_redis.src
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Logs from your program will appear here!");

            int port = 6379; // Default Redis port
            var config = new RdbConfig();
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Equals("--dir", StringComparison.OrdinalIgnoreCase))
                    {
                        config.Directory = args[i + 1];
                    }
                    if (args[i].Equals("--dbfilename", StringComparison.OrdinalIgnoreCase))
                    {
                        config.FileName = args[i + 1];
                    }
                    if (args[i].Equals("--port", StringComparison.OrdinalIgnoreCase))
                    {
                        port = Convert.ToInt32(args[i + 1]);
                    }
                }

            }


            var server = new RedisServer(port, config);

            await server.StartAsync();
        }
    }
}
