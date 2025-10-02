using ChatClient.Net;
using Jarkom.Core;
using Jarkom.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Jarkom.MVVM.ViewModel
{
    internal class MainViewModel : ObseravableObject
    {
        public ObservableCollection<MessageModel> Messages { get; set; }
        public ObservableCollection<ContactsModel> Contacts { get; set; }
        private Server _server;

        private string _username = "Username";
        public string Username
        {
            get { return _username; }
            set 
            { 
                _username = value;
                OnPropertyChanged();
            }
        }

        private string _connectionStatus = "Unconnected";
        public string ConnectionStatus
        {
            get { return _connectionStatus; }
            set 
            { 
                _connectionStatus = value;
                OnPropertyChanged();
            }
        }

        private string _connectionIcon = "/Icons/shutdown.png";
        public string ConnectionIcon
        {
            get { return _connectionIcon; }
            set
            {
                _connectionIcon = value;
                OnPropertyChanged();
            }
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                ConnectionStatus = value ? "Connected" : "Unconnected";
                ConnectionIcon = value ? "/Icons/shutdownActive.png" : "/Icons/shutdown.png";
                OnPropertyChanged();
            }
        }

        //Commands
        public RelayCommand SendCommand { get; set; }
        public RelayCommand ConnectCommand { get; set; }
        
        private ContactsModel _selectedContact;

        public ContactsModel SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                _selectedContact = value;
                OnPropertyChanged();
            }
        }



        private string _message;

        public string Message
        {
            get { return _message; }
            set 
            { 
                _message = value;
                OnPropertyChanged(); 
            }

        }
         
        public MainViewModel()
        {
            Messages = new ObservableCollection<MessageModel>();
            Contacts = new ObservableCollection<ContactsModel>();
            _server = new Server();
            _server.ConnectedEvent += userConected;
            _server.MsgReceiveEvent += MessageRecieve;
            _server.UserDisconnectedEvent += RemoveUser;

            ConnectCommand = new RelayCommand(
                o =>
                {
                    try
                    {
                        _server.ConnectToServer(Username);
                        IsConnected = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Connection failed: {ex.Message}");
                        IsConnected = false;
                    }
                },
                o => !string.IsNullOrEmpty(Username) 
            );

            SendCommand = new RelayCommand(
                o =>
                {
                    if (string.IsNullOrEmpty(Message)) return;
                    //var messageModel = new MessageModel
                    //{
                    //    Message = Message,
                    //    Time = DateTime.Now,
                    //    IsNativeOrigin = true,
                    //    Username = Username
                    //};
                    //Messages.Add(messageModel);
                    _server.SendMessageToServer(Message);
                    Message = "";
                    OnPropertyChanged();
                }
            );      
        }
        private void userConected()
        {
            var user = new ContactsModel()
            {
                Username = _server.PacketReader.ReadMessage(),
                UID = _server.PacketReader.ReadMessage(),
                ImageSource = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT5_EzMa6j7ppq2LoxtTIDQMNIfZhjYpy2Vfg&s"
            };

            if (!Contacts.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Contacts.Add(user));
            }

            var connectMessage = new MessageModel()
            {
                Message = $"{user.Username} has join the chat.",
                IsNotification = true
            };

            Application.Current.Dispatcher.Invoke(() => Messages.Add(connectMessage));
            OnPropertyChanged();
        }

        private void MessageRecieve()
        {
            var msg = _server.PacketReader.ReadMessage();
            Console.WriteLine($"Message received: {msg}"); // Debug line

            var parts = msg.Split('|');
            var username = parts.Length > 0 ? parts[0] : "Unknown";
            var uid = parts.Length > 1 ? parts[1] : "";
            var messageText = parts.Length > 2 ? parts[2] : msg;
            var isNative = uid == Contacts.FirstOrDefault(x => x.Username == Username)?.UID;
            var lastMessage = Messages.LastOrDefault();
            var isFirstMessage = lastMessage == null || lastMessage.UID != uid;

            var message = new MessageModel()
            {
                Username = username,
                UID = uid,
                ImageSource = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT5_EzMa6j7ppq2LoxtTIDQMNIfZhjYpy2Vfg&s",
                Message = messageText,
                Time = DateTime.Now,
                IsNativeOrigin = isNative,
                IsNotification = false,
                FirstMessage = isFirstMessage
            };
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(message);
                Console.WriteLine($"Messages count: {Messages.Count}"); // Debug line
            });
            OnPropertyChanged();
        }

        private void RemoveUser()
        {
            var uid = _server.PacketReader.ReadMessage();
            var user = Contacts.Where(x => x.UID == uid).FirstOrDefault();
            if (user != null)
            {
                Application.Current.Dispatcher.Invoke(() => Contacts.Remove(user));
                var connectMessage = new MessageModel()
                {
                    Message = $"{user.Username} has left the chat.",
                    IsNotification = true
                };
                Application.Current.Dispatcher.Invoke(() => Messages.Add(connectMessage));
                OnPropertyChanged();
            }
        }
    }
}
