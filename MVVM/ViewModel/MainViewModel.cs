using ChatClient.Net;
using Jarkom.Core;
using Jarkom.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jarkom.MVVM.ViewModel
{
    internal class MainViewModel : ObseravableObject
    {
        public ObservableCollection<MessageModel> Messages { get; set; }
        public ObservableCollection<ContactsModel> Contacts { get; set; }
        //Commands
        public RelayCommand SendCommand { get; set; }
        public RelayCommand ConnectToServerCommand { get; set; }
        private Server _server;

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
            //_server.ConnectToServer();
            ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer());
             
            SendCommand = new RelayCommand(o =>
            {
                Messages.Add(new MessageModel
                {
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
