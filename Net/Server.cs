using ChatClient.Net.IO;
using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ChatClient.Net
{
    public class WireMessage
    {
        public string Type { get; set; } = "";
        public string From { get; set; } = "";
        public string? To { get; set; }
        public string? Text { get; set; }
        public long Ts { get; set; }
        public string[]? Users { get; set; }
    }

    internal class Server
    {
        private readonly TcpClient _client = new();
        private NetworkStream _stream;
        private PacketReader _reader;
        private CancellationTokenSource _cts;

        public event Action<WireMessage> OnMessage;
        public bool Connected => _client.Connected;

        public async Task ConnectAsync(string host, int port, string username)
        {
            if (_client.Connected) return;

            await _client.ConnectAsync(host, port);
            _stream = _client.GetStream();
            _reader = new PacketReader(_stream);

            await SendAsync(new WireMessage { Type = "join", From = username });

            _cts = new CancellationTokenSource();
            _ = Task.Run(() => ReadLoop(_cts.Token));
        }

        private async Task ReadLoop(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var json = _reader.ReadMessage();
                    if (string.IsNullOrWhiteSpace(json)) break;

                    var msg = JsonSerializer.Deserialize<WireMessage>(json);
                    if (msg != null) OnMessage?.Invoke(msg);
                }
            }
            catch { }
        }

        public async Task SendAsync(WireMessage message)
        {
            if (!_client.Connected) return;

            var json = JsonSerializer.Serialize(message);
            var builder = new PacketBuilder();
            builder.WriteMessage(json);
            var bytes = builder.GetPacketBytes();
            await _stream.WriteAsync(bytes, 0, bytes.Length);
        }

        // reworked: dukung PM via kontak & via /w
        public async Task SendChatAsync(string from, string text, string? pmTo = null)
        {
            if (!string.IsNullOrWhiteSpace(pmTo))
            {
                await SendAsync(new WireMessage { Type = "pm", From = from, To = pmTo, Text = text });
                return;
            }

            if (text.StartsWith("/w "))
            {
                var parts = text.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    await SendAsync(new WireMessage { Type = "pm", From = from, To = parts[1], Text = parts[2] });
                    return;
                }
            }

            await SendAsync(new WireMessage { Type = "msg", From = from, Text = text });
        }

        public void Disconnect()
        {
            try { _cts?.Cancel(); } catch { }
            try { _stream?.Close(); } catch { }
            try { _client?.Close(); } catch { }
        }
    }
}
