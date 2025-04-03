namespace codecrafters_redis.src
{
    public class InfoCommand : IRedisCommand
    {
        public string Execute(string[] commandParts)
        {
            var role = "role:master";

            return $"${role.Length}\r\n{role}\r\n";
        }
    }
}
