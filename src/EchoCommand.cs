namespace codecrafters_redis.src
{
    public class EchoCommand : IRedisCommand
    {
        public string Execute(Dictionary<string, Value> dataStore, string[] commandParts)
        {
            string echoMessage = commandParts[4];
            return $"${echoMessage.Length}\r\n{echoMessage}\r\n";
        }
    }
}
