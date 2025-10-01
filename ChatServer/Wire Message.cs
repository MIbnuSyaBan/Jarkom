using System;

namespace ChatServer
{
    public class WireMessage
    {
        // JSON fields (case-sensitive sesuai client)
        public string Type { get; set; } = "";      // join|msg|pm|leave|sys|userlist
        public string From { get; set; } = "";
        public string? To { get; set; }
        public string? Text { get; set; }
        public long Ts { get; set; }

        public string[]? Users { get; set; } // khusus userlist

        public static WireMessage Sys(string text) => new() { Type = "sys", Text = text, Ts = Now() };
        public static WireMessage Join(string from) => new() { Type = "join", From = from, Ts = Now() };
        public static WireMessage Leave(string from) => new() { Type = "leave", From = from, Ts = Now() };
        public static WireMessage UserList(string[] users) => new() { Type = "userlist", Users = users, Ts = Now() };

        private static long Now() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
