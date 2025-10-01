using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ChatServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Chat Server (.NET 8)";
            var server = new TcpChatServer(IPAddress.Any, 7891);
            await server.StartAsync();
        }
    }
}
