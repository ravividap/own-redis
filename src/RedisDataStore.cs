using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis.src
{
    // Repository Pattern for data access
    public class RedisDataStore : IDataStore
    {
        private Dictionary<string, Value> data = new Dictionary<string, Value>();

        private Dictionary<int, Socket> replicas = new();

        private readonly RdbConfig config;

        private int offset { get; set; }

        public RedisDataStore(RdbConfig config)
        {
            this.config = config;

            offset = 0;

            LoadFromRdbFile();
        }

        private void LoadFromRdbFile()
        {
            var filePath = config.GetRdbFilePath();

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File {filePath} does not exist!");
                return;
            }

            try
            {
                byte[] byteData = File.ReadAllBytes(filePath);
                Console.WriteLine($"File read successfully. Data (hex): {BitConverter.ToString(byteData)}");
                ParseRedisRdbData(byteData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"An error occurred while loading contents: {ex.Message}");
            }
        }

        private void ParseRedisRdbData(byte[] data)
        {
            int index = 0;
            try
            {
                while (index < data.Length)
                {
                    if (data[index] == 0xFB) // Start of database section
                    {
                        index = ParseDatabaseSection(data, index);

                        if (data[index] == 0xFF)
                        {
                            Console.WriteLine("End of database section detected.");
                            break;
                        }

                    }
                    else
                    {
                        index++; // Skip unknown or unhandled sections
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing RDB data: {ex.Message}");
                throw;
            }
        }

        private int ParseDatabaseSection(byte[] data, int startIndex)
        {
            int index = startIndex + 1;
            int length = data[index] + data[index + 1];
            Console.WriteLine($"Database section detected. Key-value count: {length}");
            index += 2;

            for (int i = 0; i < length; i++)
            {
                ulong expiryTimeStampFC = 0;
                uint expiryTimeStampFD = 0;

                if (data[index] == 0xFC)
                {
                    index++;
                    expiryTimeStampFC = ExtractUInt64(data, ref index);

                    Console.WriteLine($"Extracted expiry information.Milliseconds information.Timestamp:{expiryTimeStampFC}");
                }

                if (data[index] == 0xFD)
                {
                    index++;
                    expiryTimeStampFD = ExtractUInt32(data, ref index);
                    Console.WriteLine($"Extracted expiry information. Seconds information. Timestamp:{expiryTimeStampFD}");
                }

                if (data[index] == 0x00)
                {
                    index++;
                    Console.WriteLine("Skipping 0x00 byte.");
                }

                if (data[index] == 0xFF)
                {
                    Console.WriteLine("End of database section detected.");
                    break;
                }

                // Parse key
                int keyLength = data[index];
                Console.WriteLine($"Key length: {keyLength}");
                index++;
                string key = ParseString(data, ref index, keyLength);
                Console.WriteLine($"Parsed key: {key}");

                // Parse value
                int valueLength = data[index];
                index++;
                string value = ParseString(data, ref index, valueLength);
                Console.WriteLine($"Parsed value: {value}");


                if (key.Length == 0)
                {
                    Console.WriteLine("Empty key found. Skipping.");
                    continue;
                }
                if (this.data.ContainsKey(key))
                {
                    this.data[key] = new Value { Data = value };
                    Console.WriteLine($"Key-Value pair updated: {key} => {value}");
                    continue;
                }


                this.data.Add(key, new Value { Data = value });
                Console.WriteLine($"Key-Value pair added: {key} => {value}");

                if (expiryTimeStampFC != 0)
                {
                    _ = HandleTimeStampExpiry((long)expiryTimeStampFC, key, false);
                    expiryTimeStampFC = 0;
                }
                else if (expiryTimeStampFD != 0)
                {
                    _ = HandleTimeStampExpiry(expiryTimeStampFD, key, true);
                    expiryTimeStampFD = 0;
                }
            }
            return index;
        }

        async Task HandleTimeStampExpiry(long unixTimeStamp, string key, bool isSeconds)
        {
            long currentUnixTime = isSeconds
                ? DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            long delay = unixTimeStamp - currentUnixTime;
            
            Console.WriteLine($"key: {key} Delay: {delay} unixTimeStamp: {unixTimeStamp} Now: {currentUnixTime}");
            
            if (delay <= 0)
            {
                Console.WriteLine($"Expiry time has already passed. Removing key. Done: {data.Remove(key)}");
                return;
            }

            await Task.Delay((int)delay);
            data.Remove(key);
        }

        static ulong ExtractUInt64(byte[] data, ref int index)
        {
            if (index + 8 >= data.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index), "Index out of range for extracting UInt64.");
            }
            ulong value = BitConverter.ToUInt64(data, index);
            index += 8;
            return value;
        }
        static uint ExtractUInt32(byte[] data, ref int index)
        {
            if (index + 4 >= data.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index), "Index out of range for extracting UInt32.");
            }
            uint value = BitConverter.ToUInt32(data, index);
            index += 4;
            return value;
        }

        private string ParseString(byte[] data, ref int index, int length)
        {
            string result = Encoding.Default.GetString(data.Skip(index).Take(length).ToArray());
            index += length;
            return result;
        }

        public Dictionary<string, Value> GetData()
        {
            return data;
        }

        public void SaveToRdbFile()
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, Socket> GetReplicas()
        {
            return replicas;
        }

        public void SetOffSet(int offSet)
        {
            offset += offSet;
        }

        public int GetOffSet()
        {
            return offset;
        }
    }
}
