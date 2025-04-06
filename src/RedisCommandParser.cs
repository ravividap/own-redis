using System.Text;

namespace codecrafters_redis.src
{
    public class RedisCommandParser
    {
        private byte[] _buffer = new byte[8192]; // Initial buffer size
        private int _bufferPos = 0;
        private int _bufferLen = 0;

        /// <summary>
        /// Process incoming data from Redis master and extract commands
        /// </summary>
        /// <param name="data">Incoming bytes from socket</param>
        /// <returns>List of parsed Redis commands, each command is a list of strings (command and arguments)</returns>
        public List<List<string>> ProcessData(byte[] data, int length)
        {
            // Add new data to buffer
            EnsureBufferCapacity(length);
            Array.Copy(data, 0, _buffer, _bufferLen, length);
            _bufferLen += length;

            List<List<string>> commands = new List<List<string>>();

            // Try to parse commands from buffer
            while (true)
            {
                List<string> command = TryParseCommand();
                if (command == null)
                    break;

                commands.Add(command);
            }

            // Compact buffer: move unparsed data to the beginning
            if (_bufferPos > 0 && _bufferPos < _bufferLen)
            {
                Array.Copy(_buffer, _bufferPos, _buffer, 0, _bufferLen - _bufferPos);
                _bufferLen -= _bufferPos;
                _bufferPos = 0;
            }
            else if (_bufferPos == _bufferLen)
            {
                _bufferPos = 0;
                _bufferLen = 0;
            }

            return commands;
        }

        /// <summary>
        /// Try to parse a single command from the buffer
        /// </summary>
        /// <returns>Parsed command or null if more data is needed</returns>
        private List<string> TryParseCommand()
        {
            if (_bufferPos >= _bufferLen)
                return null;

            int startPos = _bufferPos;

            // Check buffer starts with '*' (array)
            if (_buffer[_bufferPos] != '*')
            {
                throw new FormatException($"Expected array marker '*', got '{(char)_buffer[_bufferPos]}' at position {_bufferPos}");
            }
            _bufferPos++;

            // Parse array length
            int? arrayLength = ReadInteger();
            if (!arrayLength.HasValue)
            {
                _bufferPos = startPos; // Reset position
                return null;
            }

            List<string> command = new List<string>(arrayLength.Value);

            // Parse each array element
            for (int i = 0; i < arrayLength.Value; i++)
            {
                // Check if buffer has enough data
                if (_bufferPos >= _bufferLen)
                {
                    _bufferPos = startPos; // Reset position
                    return null;
                }

                // Check element starts with '$' (bulk string)
                if (_buffer[_bufferPos] != '$')
                {
                    throw new FormatException($"Expected bulk string marker '$', got '{(char)_buffer[_bufferPos]}' at position {_bufferPos}");
                }
                _bufferPos++;

                // Parse string length
                int? strLength = ReadInteger();
                if (!strLength.HasValue)
                {
                    _bufferPos = startPos; // Reset position
                    return null;
                }

                // Check if buffer has enough data for the string and CRLF
                if (_bufferPos + strLength.Value + 2 > _bufferLen)
                {
                    _bufferPos = startPos; // Reset position
                    return null;
                }

                // Read the string
                string element = Encoding.UTF8.GetString(_buffer, _bufferPos, strLength.Value);
                _bufferPos += strLength.Value;

                // Skip CRLF
                if (_buffer[_bufferPos] != '\r' || _buffer[_bufferPos + 1] != '\n')
                {
                    throw new FormatException($"Expected CRLF, got '{(char)_buffer[_bufferPos]}{(char)_buffer[_bufferPos + 1]}' at position {_bufferPos}");
                }
                _bufferPos += 2;

                command.Add(element);
            }

            return command;
        }

        public string ConvertListToResp(List<string> list)
        {
            if (list == null)
                return "$-1\r\n"; // Null bulk string

            StringBuilder respBuilder = new StringBuilder();

            // Add array header
            respBuilder.Append("*").Append(list.Count).Append("\r\n");

            // Add each element as a bulk string
            foreach (string item in list)
            {
                if (item == null)
                {
                    respBuilder.Append("$-1\r\n"); // Null bulk string
                }
                else
                {
                    byte[] itemBytes = Encoding.UTF8.GetBytes(item);
                    respBuilder.Append("$").Append(itemBytes.Length).Append("\r\n");
                    respBuilder.Append(item).Append("\r\n");
                }
            }

            return respBuilder.ToString();
        }

        /// <summary>
        /// Read integer value terminated by CRLF
        /// </summary>
        /// <returns>Integer value or null if more data is needed</returns>
        private int? ReadInteger()
        {
            int startPos = _bufferPos;
            bool isNegative = false;

            // Find CRLF
            int endPos = -1;
            for (int i = startPos; i < _bufferLen - 1; i++)
            {
                if (_buffer[i] == '\r' && _buffer[i + 1] == '\n')
                {
                    endPos = i;
                    break;
                }
            }

            if (endPos == -1)
                return null; // Need more data

            // Check for negative number
            if (_buffer[startPos] == '-')
            {
                isNegative = true;
                startPos++;
            }

            // Parse the integer
            int value = 0;
            for (int i = startPos; i < endPos; i++)
            {
                if (_buffer[i] < '0' || _buffer[i] > '9')
                    throw new FormatException($"Invalid character in integer: '{(char)_buffer[i]}' at position {i}");

                value = value * 10 + (_buffer[i] - '0');
            }

            _bufferPos = endPos + 2; // Skip CRLF
            return isNegative ? -value : value;
        }

        /// <summary>
        /// Ensure buffer has enough capacity for new data
        /// </summary>
        private void EnsureBufferCapacity(int additionalLength)
        {
            if (_bufferLen + additionalLength > _buffer.Length)
            {
                // Determine new buffer size (at least double or enough for the new data)
                int newSize = Math.Max(_buffer.Length * 2, _bufferLen + additionalLength);
                byte[] newBuffer = new byte[newSize];
                Array.Copy(_buffer, 0, newBuffer, 0, _bufferLen);
                _buffer = newBuffer;
            }
        }

        /// <summary>
        /// Example usage showing how to handle multiple commands in a single segment
        /// </summary>
    }
}
