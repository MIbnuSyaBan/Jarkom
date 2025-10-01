using ChatClient.Net;
using Jarkom.Core;
using Jarkom.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            
            ConnectCommand = new RelayCommand(o =>
            {
                try
                {
                    _server.ConnectToServer();
                    IsConnected = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Connection failed: {ex.Message}");
                    IsConnected = false;
                }
            });
             
            SendCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrEmpty(Message)) return;
                
                Messages.Add(new MessageModel
                {
                    Username = Username,
                    Message = Message,
                    FirstMessage = false
                });

                Message = "";
            });


            Messages.Add(new MessageModel()
            {
                Username = "System",
                UsernameColor = "#888888",
                ImageSource = "https://i.imgur.com/yMWvLXd.png", // Or use a system/default icon
                Message = "Lupopou has joined the chat.",
                Time = DateTime.Now,
                IsNativeOrigin = false,
                FirstMessage = true,
                IsNotification = true
            });


            Messages.Add(new MessageModel()
            {
                Username = "Lupopou",
                UsernameColor = "#409AFF",
                ImageSource = "https://i.imgur.com/yMWvLXd.png",
                Message = "Hello, how are you?",
                Time = DateTime.Now,
                IsNativeOrigin = false,
                FirstMessage = true,
                IsNotification = false
            });
            Messages.Add(new MessageModel()
            {
                Username = "Lupopou",
                UsernameColor = "#409AFF",
                ImageSource = "https://i.imgur.com/yMWvLXd.png",
                Message = "fine",
                Time = DateTime.Now,
                IsNativeOrigin = true,
                FirstMessage = false,
                IsNotification = false
            });


            for (int i = 0; i < 1; i++)
            {
                Contacts.Add(new ContactsModel()
                {
                    Username = $"Lupopou {i}",
                    ImageSource = "https://i.imgur.com/yMWvLXd.png",
                    Messages = Messages
                });
            }
        }
    }
}
