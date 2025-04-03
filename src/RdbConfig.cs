namespace codecrafters_redis.src
{
    public class RdbConfig
    {
        public string? Directory { get; set; }

        public string? FileName { get; set; }

        public string GetRdbFilePath()
        {
            return System.IO.Path.Combine(Directory, FileName);
        }
    }
}
