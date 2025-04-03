namespace codecrafters_redis.src
{
    public interface IDataStore
    {
        Dictionary<string, Value> GetData();
        void SaveToRdbFile();
    }
}
