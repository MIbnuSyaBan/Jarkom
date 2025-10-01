using System.IO;
using System.Text;

namespace ChatClient.Net.IO
{
    class PacketBuilder
    {
        MemoryStream _ms = new MemoryStream();

        public void WriteOpCode(byte opcode) => _ms.WriteByte(opcode); // (opsional, tak dipakai sekarang)

        public void WriteMessage(string msg)
        {
            // gunakan panjang byte UTF8 (BUKAN msg.Length)
            var data = Encoding.UTF8.GetBytes(msg);
            _ms.Write(System.BitConverter.GetBytes(data.Length), 0, 4);
            _ms.Write(data, 0, data.Length);
        }

        public byte[] GetPacketBytes() => _ms.ToArray();
    }
}
