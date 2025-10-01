using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

namespace ChatServer
{
    public class TcpChatServer
    {
        private readonly TcpListener _listener;
        private readonly ConcurrentDictionary<string, ClientConnection> _clients = new();

        public TcpChatServer(IPAddress ip, int port)
        {
            _listener = new TcpListener(ip, port);
        }

        public async Task StartAsync(CancellationToken ct = default)
        {
            _listener.Start();
            Console.WriteLine($"[INFO] Server started on {_listener.LocalEndpoint}");

            while (!ct.IsCancellationRequested)
            {
                var tcp = await _listener.AcceptTcpClientAsync(ct);
                _ = HandleClientAsync(new ClientConnection(tcp));
            }
        }

        private async Task HandleClientAsync(ClientConnection conn)
        {
            try
            {
                var join = await conn.ReadAsync<WireMessage>();
                if (join?.Type != "join" || string.IsNullOrWhiteSpace(join.From))
                {
                    await conn.WriteAsync(WireMessage.Sys("Username required. Send type=join first."));
                    conn.Dispose();
                    return;
                }

                var username = join.From.Trim();
                if (!_clients.TryAdd(username, conn))
                {
                    await conn.WriteAsync(WireMessage.Sys("Username already taken."));
                    conn.Dispose();
                    return;
                }

                conn.Username = username;
                Console.WriteLine($"[INFO] {username} connected.");

                await BroadcastAsync(WireMessage.Join(username));
                await BroadcastUserListAsync();

                while (true)
                {
                    var msg = await conn.ReadAsync<WireMessage>();
                    if (msg == null) break;

                    msg.From = username;
                    msg.Ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    switch (msg.Type)
                    {
                        case "msg":
                            // broadcast ke semua kecuali pengirim → supaya tidak dobel
                            await BroadcastAsync(msg, exceptUsername: username);
                            break;

                        case "pm":
                            if (string.IsNullOrWhiteSpace(msg.To) || !_clients.TryGetValue(msg.To, out var dst))
                            {
                                await conn.WriteAsync(WireMessage.Sys($"User '{msg.To}' not found."));
                            }
                            else
                            {
                                // hanya kirim ke target, pengirim tidak di-echo (client sudah tampilkan sendiri)
                                await dst.WriteAsync(msg);
                            }
                            break;

                        default:
                            await conn.WriteAsync(WireMessage.Sys("Unknown type."));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERR] {ex.Message}");
            }
            finally
            {
                if (!string.IsNullOrEmpty(conn.Username) && _clients.TryRemove(conn.Username, out _))
                {
                    Console.WriteLine($"[INFO] {conn.Username} disconnected.");
                    _ = BroadcastAsync(WireMessage.Leave(conn.Username));
                    _ = BroadcastUserListAsync();
                }
                conn.Dispose();
            }
        }

        private Task BroadcastAsync(WireMessage message, string? exceptUsername = null)
        {
            var tasks = new List<Task>(_clients.Count);
            foreach (var kv in _clients)
            {
                if (exceptUsername != null && kv.Key.Equals(exceptUsername, StringComparison.OrdinalIgnoreCase))
                    continue;

                tasks.Add(kv.Value.WriteAsync(message));
            }
            return Task.WhenAll(tasks);
        }

        private Task BroadcastUserListAsync()
        {
            var users = _clients.Keys.ToArray();
            var msg = WireMessage.UserList(users);
            return BroadcastAsync(msg);
        }
    }
}
