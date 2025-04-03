namespace codecrafters_redis.src
{
    public class InfoCommand(bool isSlave) : IRedisCommand
    {
        public string Execute(string[] commandParts)
        {
            var role = "role:" + (isSlave ? "slave" : "master");

            return $"${role.Length}\r\n{role}\r\n";
        }
    }
}
