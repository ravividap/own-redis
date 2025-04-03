namespace codecrafters_redis.src
{
    public class Value
    {
        public string? Data { get; set; }

        public long? ExpireAt { get; set; }
        
        public DateTime? ExpiresAtDateTime { get; set; }

        public DateTime? Expiry { get; set; }
    }
}
