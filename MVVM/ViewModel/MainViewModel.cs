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
                o => !string.IsNullOrEmpty(Username) // ini sama seperti di contoh pertama
            );

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
        }
        private void userConected()
        {
            var user = new ContactsModel()
            {
                Username = _server.PacketReader.ReadMessage(),
                UID = _server.PacketReader.ReadMessage(),
                ImageSource = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT5_EzMa6j7ppq2LoxtTIDQMNIfZhjYpy2Vfg&s",
            };

            if (!Contacts.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Contacts.Add(user));
            }
        }
    }
}
