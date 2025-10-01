using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ChatClient.Net.IO
{
    class PacketReader : BinaryReader
    {
        private readonly NetworkStream _ns;

        public PacketReader(NetworkStream ns) : base(ns) => _ns = ns;

        public string ReadMessage()
        {
            var length = ReadInt32();
            if (length <= 0 || length > 1024 * 1024) return string.Empty;

            var buffer = new byte[length];
            int read = 0;
            while (read < length)
            {
                int n = _ns.Read(buffer, read, length - read);
                if (n == 0) break;
                read += n;
            }
            return Encoding.UTF8.GetString(buffer, 0, read);
        }
    }
}
