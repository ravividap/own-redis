using System.Net.Sockets;

namespace codecrafters_redis.src
{
    public interface IDataStore
    {
        Dictionary<string, Value> GetData();
        void SaveToRdbFile();

        Dictionary<int, Socket> GetReplicas();

        void SetOffSet(int offSet);

        int GetOffSet();
    }
}
