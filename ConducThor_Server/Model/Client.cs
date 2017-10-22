using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ConducThor_Server.Annotations;

namespace ConducThor_Server.Model
{
    public class ClientViewmodel : INotifyPropertyChanged
    {
        private String _id = String.Empty;

        public String ID
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged();}
        }
        
        public String CurrentCommand { get; set; }

        public bool Status { get; set; }
        public String StatusString { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
