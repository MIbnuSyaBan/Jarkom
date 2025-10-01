using ChatClient.Net;
using Jarkom.Core;
using Jarkom.MVVM.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Jarkom.MVVM.ViewModel
{
    internal class MainViewModel : ObseravableObject
    {
        public ObservableCollection<MessageModel> Messages { get; set; } = new();
        public ObservableCollection<ContactsModel> Contacts { get; set; } = new();

        private ContactsModel _selectedContact;
        public ContactsModel SelectedContact
        {
            get => _selectedContact;
            set { _selectedContact = value; OnPropertyChanged(); }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        private string _username = "User";
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _host = "127.0.0.1";
        public string Host
        {
            get => _host;
            set { _host = value; OnPropertyChanged(); }
        }

        private int _port = 7891;
        public int Port
        {
            get => _port;
            set { _port = value; OnPropertyChanged(); }
        }

        public RelayCommand ConnectCommand { get; set; }
        public RelayCommand DisconnectCommand { get; set; }
        public RelayCommand SendCommand { get; set; }

        private readonly Server _server = new();

        public MainViewModel()
        {
            // default room "General"
            Contacts.Add(new ContactsModel { Username = "General", ImageSource = "", Messages = Messages });
            SelectedContact = Contacts.First();

            _server.OnMessage += Server_OnMessage;

            ConnectCommand = new RelayCommand(async _ => await ConnectAsync(), _ => !_server.Connected);
            DisconnectCommand = new RelayCommand(_ => Disconnect(), _ => _server.Connected);
            SendCommand = new RelayCommand(async _ => await SendAsync(), _ => _server.Connected && !string.IsNullOrWhiteSpace(Message));
        }

        private async Task ConnectAsync()
        {
            try
            {
                await _server.ConnectAsync(Host, Port, Username);
                AppendSys($"Connected to {Host}:{Port} as {Username}");
            }
            catch (Exception ex)
            {
                AppendSys($"Failed to connect: {ex.Message}");
            }
        }

        private void Disconnect()
        {
            _server.Disconnect();
            AppendSys("Disconnected.");
        }

        private async Task SendAsync()
        {
            var text = Message?.Trim();
            if (string.IsNullOrEmpty(text)) return;

            // cek apakah ini private chat dari kontak yang dipilih
            string? pmTarget = null;
            if (SelectedContact != null &&
                !string.Equals(SelectedContact.Username, "General", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(SelectedContact.Username, Username, StringComparison.OrdinalIgnoreCase))
            {
                pmTarget = SelectedContact.Username;
            }

            // kirim pesan ke server
            await _server.SendChatAsync(Username, text, pmTarget);

            // tampilkan pesan lokal sekali (server tidak echo balik pengirim → aman)
            Messages.Add(new MessageModel
            {
                Username = Username,
                UsernameColor = "#409AFF",
                Message = (pmTarget == null) ? text : $"(to {pmTarget}) {text}",
                Time = DateTime.Now,
                IsNativeOrigin = true,
                FirstMessage = false
            });

            Message = string.Empty;
        }

        private void Server_OnMessage(WireMessage msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                switch (msg.Type)
                {
                    case "sys":
                    case "join":
                    case "leave":
                        AppendSys(RenderText(msg));
                        break;

                    case "userlist":
                        SyncUsers(msg.Users ?? Array.Empty<string>());
                        break;

                    case "msg":
                    case "pm":
                        Messages.Add(new MessageModel
                        {
                            Username = msg.From,
                            UsernameColor = "#90EE90",
                            Message = msg.Text ?? "",
                            Time = DateTime.Now,
                            IsNativeOrigin = false,
                            FirstMessage = false
                        });
                        break;
                }
            });
        }

        private string RenderText(WireMessage m)
        {
            return m.Type switch
            {
                "sys" => $"[SYS] {m.Text}",
                "join" => $"[SYS] {m.From} joined.",
                "leave" => $"[SYS] {m.From} left.",
                _ => m.Text ?? ""
            };
        }

        private void AppendSys(string text)
        {
            Messages.Add(new MessageModel
            {
                Username = "System",
                UsernameColor = "#CCCCCC",
                Message = text,
                Time = DateTime.Now,
                IsNativeOrigin = false,
                FirstMessage = false
            });
        }

        private void SyncUsers(string[] users)
        {
            // pastikan kontak online sinkron dengan daftar user dari server
            foreach (var u in users)
            {
                if (u.Equals("General", StringComparison.OrdinalIgnoreCase)) continue;
                if (!Contacts.Any(c => c.Username == u))
                {
                    Contacts.Add(new ContactsModel
                    {
                        Username = u,
                        ImageSource = "",
                        Messages = Messages // untuk sekarang 1 room bersama
                    });
                }
            }

            // hapus user yang offline
            var toRemove = Contacts.Where(c => c.Username != "General" && !users.Contains(c.Username)).ToList();
            foreach (var r in toRemove) Contacts.Remove(r);
        }
    }
}
