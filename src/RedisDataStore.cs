using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace codecrafters_redis.src
{
    // Repository Pattern for data access
    public class RedisDataStore : IDataStore
    {
        private Dictionary<string, Value> _data = new Dictionary<string, Value>();

        private readonly RdbConfig _config;

        public RedisDataStore(RdbConfig config)
        {
            _config = config;

            LoadFromRdbFile();
        }

        private void LoadFromRdbFile()
        {
            var filePath = _config.GetRdbFilePath();

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File {filePath} does not exist!");
                return;
            }

            try
            {
                byte[] byteData = File.ReadAllBytes(filePath);
                Console.WriteLine($"File read successfully. Data (hex): {BitConverter.ToString(byteData)}");
                _data = ParseRedisRdbData(byteData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"An error occurred while loading contents: {ex.Message}");
            }
        }

        private Dictionary<string, Value> ParseRedisRdbData(byte[] data)
        {
            Dictionary<string, Value> keyValuePairs = new();
            int index = 0;
            try
            {
                while (index < data.Length)
                {
                    if (data[index] == 0xFB) // Start of database section
                    {
                        index = ParseDatabaseSection(data, index, keyValuePairs);
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
            return keyValuePairs;
        }

        private int ParseDatabaseSection(byte[] data, int startIndex, Dictionary<string, Value> keyValuePairs)
        {
            int index = startIndex + 1;
            int length = data[index] + data[index + 1];
            Console.WriteLine($"Database section detected. Key-value count: {length}");
            index += 2;
            if (data[index] != 0x00)
            {
                throw new InvalidOperationException("Non-string types are not supported yet.");
            }
            index++;
            for (int i = 0; i < length; i++)
            {
                if (data[index] == 0xFC)
                {
                    Console.WriteLine("Skipping expiry information.");
                    index += 10; // Skip FC + 8-byte unsigned long + 0x00
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
                keyValuePairs.Add(key, new Value { Data = value });
                Console.WriteLine($"Key-Value pair added: {key} => {value}");
            }
            return index;
        }

        private string ParseString(byte[] data, ref int index, int length)
        {
            string result = Encoding.Default.GetString(data.Skip(index).Take(length).ToArray());
            index += length;
            return result;
        }

        public Dictionary<string, Value> GetData()
        {
            return _data;
        }

        public void SaveToRdbFile()
        {
            throw new NotImplementedException();
        }
    }
}
