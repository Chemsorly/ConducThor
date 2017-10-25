using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using ConducThor_Server.Annotations;
using ConducThor_Server.Model;
using ConducThor_Server.Server;

namespace ConducThor_Server
{
    public class MainWindowViewmodel : INotifyPropertyChanged
    {
        private SignalRManager _signalrmanager;
        public ObservableCollection<ClientViewmodel> ClientList { get; set; }

        private Dispatcher dispatcher;

        public void Initialize()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            ClientList = new ObservableCollection<ClientViewmodel>();

            _signalrmanager = new SignalRManager();
            _signalrmanager.NewClientEvent += pClient =>
            {
                dispatcher.Invoke(() =>
                {
                    this.ClientList.Add(new ClientViewmodel() {ID = pClient.ID });
                });
            };
            _signalrmanager.ClientDisconnectedEvent += pClient =>
            {
                dispatcher.Invoke(() =>
                {
                    this.ClientList.Remove(this.ClientList.First(t => t.ID == pClient.ID));
                });
            };
            _signalrmanager.ClientUpdatedEvent += delegate(Client pClient)
            {
                dispatcher.Invoke(() =>
                {
                    var client = this.ClientList.FirstOrDefault(t => t.ID == pClient.ID);
                    if (client != null)
                    {
                        client.UpdateValues(pClient);
                    }
                });
            };


            _signalrmanager.Initialize();

            OnPropertyChanged(String.Empty);
        }


        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
              PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
