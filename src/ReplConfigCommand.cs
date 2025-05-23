﻿using System.Net.Sockets;
using System.Text;

namespace codecrafters_redis.src
{
    public class ReplConfigCommand(IDataStore dataStore) : IRedisCommand
    {
        public async Task ExecuteAsync(Socket client, string[] commandParts)
        {
            string getack = commandParts[4];

            var offSet = dataStore.GetOffSet().ToString();
            dataStore.SetOffSet(37);
            var response = $"*3\r\n$8\r\nREPLCONF\r\n$3\r\nACK\r\n${offSet.Length}\r\n{offSet}\r\n";

            await client.SendAsync(Encoding.UTF8.GetBytes(response), SocketFlags.None);

            //if (getack.Equals("GETACK", StringComparison.OrdinalIgnoreCase))
            //{
            //    var offSet = dataStore.GetOffSet().ToString();
            //    dataStore.SetOffSet(37);
            //    await client.SendAsync(Encoding.UTF8.GetBytes($"*3\r\n$8\r\nREPLCONF\r\n$3\r\nACK\r\n${offSet.Length}\r\n{offSet}\r\n"), SocketFlags.None);
            //}
            //else
            //{
            //    await client.SendAsync(Encoding.UTF8.GetBytes("+OK\r\n"), SocketFlags.None);
            //}
        }
    }
}
