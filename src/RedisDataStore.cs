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
            int index = startIndex + 1; // Skip the FE marker
            int dbNumber = data[index]; // Get database number
            Console.WriteLine($"Parsing database {dbNumber}");
            index++;

            // Read key count and expired key count
            int keyCount = data[index];
            index++;
            int expiredKeyCount = data[index];
            index++;

            Console.WriteLine($"Database contains {keyCount} keys, {expiredKeyCount} expired keys");

            for (int i = 0; i < keyCount; i++)
            {
                long? expiryTime = null;
                DateTime? expiryDateTime = null;

                // Check for expiry information
                if (data[index] == 0xFC)
                {
                    // Expiry in milliseconds
                    index++;
                    expiryTime = BitConverter.ToInt64(data, index);
                    expiryDateTime = DateTimeOffset.FromUnixTimeMilliseconds((long)expiryTime).DateTime.ToLocalTime();
                    Console.WriteLine($"Found expiry information (milliseconds): {expiryDateTime}");
                    index += 8;
                }
                else if (data[index] == 0xFD)
                {
                    // Expiry in seconds
                    index++;
                    int secondsExpiry = BitConverter.ToInt32(data, index);
                    expiryTime = secondsExpiry;
                    expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(secondsExpiry).DateTime.ToLocalTime();
                    Console.WriteLine($"Found expiry information (seconds): {expiryDateTime}");
                    index += 4;
                }

                // Check value type
                if (data[index] != 0x00)
                {
                    Console.WriteLine($"Unsupported value type: 0x{data[index]:X2}");
                    throw new NotSupportedException($"Only string values are supported. Found type 0x{data[index]:X2}");
                }
                index++; // Skip the value type byte

                // Parse key
                int keyLength = data[index];
                index++;
                string key = Encoding.UTF8.GetString(data, index, keyLength);
                index += keyLength;

                // Parse value
                int valueLength = data[index];
                index++;
                string value = Encoding.UTF8.GetString(data, index, valueLength);
                index += valueLength;

                Console.WriteLine($"Parsed key: '{key}', value: '{value}'" +
                                 (expiryTime.HasValue ? $", expires: {expiryDateTime}" : ""));

                // Store in dictionary
                var valueObj = new Value
                {
                    Data = value,
                    ExpireAt = expiryTime,
                    ExpiresAtDateTime = expiryDateTime
                };

                if (keyValuePairs.ContainsKey(key))
                {
                    keyValuePairs[key] = valueObj;
                }
                else
                {
                    keyValuePairs.Add(key, valueObj);
                }
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
