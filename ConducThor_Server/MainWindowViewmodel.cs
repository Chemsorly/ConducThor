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
        private SignalRManager _signalrmanager;
        public ObservableCollection<ClientViewmodel> ClientList { get; set; }

        private Dispatcher dispatcher;
        private UpdateNotifier _updateNotifier;

        private List<String> LogMessages = new List<string>();
        public String Log => String.Join("\n", LogMessages);

        private ClientViewmodel _selectedClient;
        public ClientViewmodel SelectedClient
        {
            get { return _selectedClient; }
            set { _selectedClient = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedClientLogMessages)); }
        }

        public AsyncObservableCollection<String> SelectedClientLogMessages => SelectedClient?.LogMessages;

        public String VersionStatus => _updateNotifier == null ? String.Empty : (_updateNotifier.Status == Utility.VersionStatus.UpdateAvailable ? " Update available!": String.Empty);
        public void Initialize()
        {
            //int
            dispatcher = Dispatcher.CurrentDispatcher;
            ClientList = new ObservableCollection<ClientViewmodel>();

            //init updater
            _updateNotifier = new UpdateNotifier();
            _updateNotifier.PropertyChanged += (sender, args) => OnPropertyChanged(nameof(VersionStatus));

           _signalrmanager = new SignalRManager();
            _signalrmanager.NewClientEvent += pClient =>
            {
                dispatcher.Invoke(() =>
                {
                    this.ClientList.Add(new ClientViewmodel(pClient) {ID = pClient.ID });
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
            _signalrmanager.NewLogMessageEvent += delegate(string message)
            {
                dispatcher.Invoke(() =>
                {
                    if (message != null)
                    {
                        LogMessages.Add($"[{DateTime.UtcNow:G}] {message}");
                        OnPropertyChanged(nameof(Log));
                    }
                });
            };
            _signalrmanager.NewConsoleLogMessage += delegate(Client pClient, string message)
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
