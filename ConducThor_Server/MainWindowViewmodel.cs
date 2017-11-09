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
using ConducThor_Server.Utility;

namespace ConducThor_Server
{
    public class MainWindowViewmodel : INotifyPropertyChanged
    {
        private Core _core;

        public ObservableCollection<ClientViewmodel> ClientList { get; set; }
        private Dispatcher dispatcher;

        private List<String> LogMessages = new List<string>();
        public String Log => String.Join("\n", LogMessages);

        private ClientViewmodel _selectedClient;
        public ClientViewmodel SelectedClient
        {
            get { return _selectedClient; }
            set { _selectedClient = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(SelectedClientLogMessages)); }
        }

        public AsyncObservableCollection<String> SelectedClientLogMessages => SelectedClient?.LogMessages;
        public String VersionStatus => _core?.VersionStatus;
        public String ConnectedClientsString => $"Connected Clients: {ClientList?.Count.ToString()}";

        //Queue operations



        public void Initialize()
        {
            //int
            dispatcher = Dispatcher.CurrentDispatcher;
            ClientList = new ObservableCollection<ClientViewmodel>();
            _core = new Core();
            _core.PropertyChanged += (sender, args) => PropertyChanged?.Invoke(sender, args);

            _core.NewClientEvent += pClient =>
            {
                dispatcher.Invoke(() =>
                {
                    this.ClientList.Add(new ClientViewmodel(pClient) {ID = pClient.ID });
                    NotifyPropertyChanged(nameof(ConnectedClientsString));
                });
            };
            _core.ClientDisconnectedEvent += pClient =>
            {
                dispatcher.Invoke(() =>
                {
                    this.ClientList.Remove(this.ClientList.First(t => t.ID == pClient.ID));
                    NotifyPropertyChanged(nameof(ConnectedClientsString));
                });
            };
            _core.ClientUpdatedEvent += delegate(Client pClient)
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
            _core.NewLogMessageEvent += delegate(string message)
            {
                dispatcher.Invoke(() =>
                {
                    if (message != null)
                    {
                        LogMessages.Add($"[{DateTime.UtcNow:G}] {message}");
                        NotifyPropertyChanged(nameof(Log));
                    }
                });
            };
            _core.NewConsoleLogMessage += delegate(Client pClient, string message)
                {
                    dispatcher.Invoke(() =>
                    {
                        var client = this.ClientList.FirstOrDefault(t => t.ID == pClient.ID);
                        if (client != null && !String.IsNullOrWhiteSpace(message))
                        {
                            client.LogMessages.Add($"[{DateTime.UtcNow:G}] {message}");
                        }
                    });
                };

            _core.Initialize();
            NotifyPropertyChanged(String.Empty);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
              PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
