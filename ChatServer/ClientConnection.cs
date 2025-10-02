using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatServer
{
    public class ClientConnection : IDisposable
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;

        public string Username { get; set; } = string.Empty;

        public ClientConnection(TcpClient client)
        {
            _client = client;
            _client.NoDelay = true;
            _stream = _client.GetStream();
        }

        /// <summary>
        /// Baca payload generic dari stream (pakai prefix length)
        /// </summary>
        public async Task<T?> ReadAsync<T>() where T : class
        {
            var lenBuf = new byte[4];
            int r = await _stream.ReadAsync(lenBuf, 0, 4);
            if (r == 0) return null;

            int length = BitConverter.ToInt32(lenBuf, 0);
            if (length <= 0 || length > 1024 * 1024) return null;

            var data = new byte[length];
            int read = 0;
            while (read < length)
            {
                int n = await _stream.ReadAsync(data, read, length - read);
                if (n == 0) return null;
                read += n;
            }

            var json = Encoding.UTF8.GetString(data);

            // Deserialisasi wire message (chat / typing / dsb.)
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// Kirim payload generic ke client
        /// </summary>
        public async Task WriteAsync<T>(T payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var data = Encoding.UTF8.GetBytes(json);
            var len = BitConverter.GetBytes(data.Length);
            await _stream.WriteAsync(len, 0, 4);
            await _stream.WriteAsync(data, 0, data.Length);
        }

        /// <summary>
        /// Shortcut kirim status typing
        /// </summary>
        public async Task SendTypingAsync(string username, bool isTyping)
        {
            var msg = new
            {
                Type = "typing",
                From = username,
                Text = isTyping ? "typing..." : ""
            };

            await WriteAsync(msg);
        }

        public void Dispose()
        {
            try { _stream?.Close(); } catch { }
            try { _client?.Close(); } catch { }
        }
    }
}
