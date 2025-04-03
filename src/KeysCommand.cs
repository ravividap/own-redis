namespace codecrafters_redis.src
{
    public class KeysCommand(IDataStore dataStore) : IRedisCommand
    {
        
        public string Execute(string[] commandParts)
        {
            Console.WriteLine(commandParts[0]);
            Console.WriteLine(commandParts[1]);
            Console.WriteLine(commandParts[2]);

            if (commandParts[3] == "*")
            {
                return BuildArrayString(dataStore.GetData().Keys.ToArray());
            }

            return "$-1\r\n";
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
