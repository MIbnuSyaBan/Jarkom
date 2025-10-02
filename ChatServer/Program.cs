using ChatServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ChatServer
{
    class Program
    {
        static List<Client> _users;
        static TcpListener _listener;

        static void Main(string[] args)
        {
            _users = new List<Client>();
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            _listener.Start();

            while (true)
            {
                var client = new Client(_listener.AcceptTcpClient());
                _users.Add(client);

                //Broadcast that a new user has connected to all users
                BroadcastConnection();
            }
        }

        static void BroadcastConnection()
        {
            foreach (var user in _users.ToList()) // Create a copy to avoid modification during enumeration
            {
                try
                {
                    foreach (var usr in _users)
                    {
                        var broadcastPacket = new PacketBuilder();
                        broadcastPacket.WriteOpCode(1);
                        broadcastPacket.WriteMessage(usr.Username);
                        broadcastPacket.WriteMessage(usr.UID.ToString());
                        user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                    }
                }
                catch (Exception)
                {
                    _users.Remove(user); // Remove disconnected users
                }
            }
        }

        public static void BroadcastMessage(string msg)
        {
            foreach (var user in _users.ToList())  
            {
                var broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode(5);
                broadcastPacket.WriteMessage(msg);
                user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());             
            }
        }

        public static void BroadcastDisconnect(string uid)
        {
            var disconnectedUser = _users.Where(x => x.UID.ToString() == uid).FirstOrDefault();
            _users.Remove(disconnectedUser);

            foreach (var user in _users)
            {
                var broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode(10);
                broadcastPacket.WriteMessage(uid);
                user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
            }

            //BroadcastMessage($"{disconnectedUser.Username} Disconnected!");
        }

    }
}
