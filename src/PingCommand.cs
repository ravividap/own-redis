﻿using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis.src
{
    public class PingCommand(IDataStore dataStore, bool isSlave) : IRedisCommand
    {
        public async Task ExecuteAsync(Socket client, string[] commandParts)
        {
            dataStore.SetOffSet(14);
            await client.SendAsync(Encoding.UTF8.GetBytes("+PONG\r\n"), SocketFlags.None);
        }
    }
}
