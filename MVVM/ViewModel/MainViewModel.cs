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
                Username = "Lupopou",
                UsernameColor = "#409AFF",
                ImageSource = "https://i.imgur.com/yMWvLXd.png",
                Message = "Hello, how are you?",
                Time = DateTime.Now,
                IsNativeOrigin = false,
                FirstMessage = true
            });
            for (int i = 0; i < 3; i++)
            {
                Messages.Add(new MessageModel()
                {
                    Username = "X",
                    UsernameColor = "#409AFF",
                    ImageSource = "https://i.imgur.com/yMWvLXd.png",
                    Message = "I'm fine, thank you!",
                    Time = DateTime.Now,
                    IsNativeOrigin = false,
                    FirstMessage = false
                });
            }
            for (int i = 0; i < 2; i++)
            {
                Messages.Add(new MessageModel()
                {
                    Username = "Lupopou",
                    UsernameColor = "#409AFF",
                    ImageSource = "https://i.imgur.com/yMWvLXd.png",
                    Message = "What about you?",
                    Time = DateTime.Now,
                    IsNativeOrigin = true,
                });
            }
            Messages.Add(new MessageModel()
            {
                Username = "Lupopou",
                UsernameColor = "#409AFF",
                ImageSource = "https://i.imgur.com/yMWvLXd.png",
                Message = "Last",
                Time = DateTime.Now,
                IsNativeOrigin = true,
            });

            for (int i = 0; i < 5; i++)
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
