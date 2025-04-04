namespace codecrafters_redis.src
{
    public class PsyncCommand(string replicationId, int offset) : IRedisCommand
    {
        public string Execute(string[] commandParts)
        {
            Console.WriteLine();

            return $"+FULLRESYNC {replicationId} {offset}\r\n";
        }
    }
}
