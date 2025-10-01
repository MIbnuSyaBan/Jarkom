using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jarkom.MVVM.Model
{
    class ContactsModel
    {
        public string Username { get; set; }
        public string UID { get; set; }
        public string ImageSource { get; set; }
        public ObservableCollection<MessageModel> Messages { get; set; }
        public string LastMessage => (Messages != null && Messages.Count > 0) ? Messages.Last().Message : string.Empty;
    }
}
