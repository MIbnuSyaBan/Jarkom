using ChatClient.Net.IO;
using System;
using System.Net.Sockets;
using System.IO;

namespace ChatClient.Net
{

    internal class Server
    {
        TcpClient _client;
        public PacketReader PacketReader;

        public event Action ConnectedEvent;

        public Server()
        {
            _client = new TcpClient();

        }
        public void ConnectToServer(string username)
        {
            if (!_client.Connected)
            {
                _client.Connect("127.0.0.1", 7891);
                PacketReader = new PacketReader(_client.GetStream());
                if (!string.IsNullOrEmpty(username))
                {
                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteMessage(username);
                    _client.Client.Send(connectPacket.GetPacketBytes());
                }
                ReadPackets();
               
            }
        }
        private void ReadPackets()
        {
            Task.Run(() =>
            {
                try
                {
                    while (_client.Connected)
                    {
                        if (_client.Available > 0)
                        {
                            var opcode = PacketReader.ReadByte();
                            switch (opcode)
                            {
                                case 1:
                                    ConnectedEvent?.Invoke();
                                    break;
                                default:
                                    Console.WriteLine("ah yes..");
                                    break;
                            }
                        }
                        else
                        {
                            Task.Delay(10).Wait(); // Add a small delay to prevent CPU spinning
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is SocketException)
                {
                    // Handle disconnection
                    Console.WriteLine($"Disconnected from server: {ex.Message}");
                }
                finally
                {
                    _client.Close();
                }
            });

        }
    }
}
