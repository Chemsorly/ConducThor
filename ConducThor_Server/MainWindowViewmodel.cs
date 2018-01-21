using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using ConducThor_Server.Annotations;
using ConducThor_Server.Commands;
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

        private Queue<String> LogMessages = new Queue<string>();
        public String Log => String.Join("\n", LogMessages.Reverse());

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
        public AsyncObservableCollection<WorkItem> QueuedWorkItems => _core?.QueuedWorkItems;
        public AsyncObservableCollection<WorkItem> ActiveWorkItems => _core?.ActiveWorkItems;
        public String QueuedOperationsCount => $"Queued Operations: {QueuedWorkItems?.Count}";
        public String ActiveOperationsCount => $"Active Operations: {ActiveWorkItems?.Count}";

        //ui
        public ICommand GenerateWorkCommand { get; set; }


        public void Initialize()
        {
            //ui TODO fix error when multithreaded
            this.GenerateWorkCommand = new RelayCommand<object>(GenerateWork, false);

            //int
            dispatcher = Dispatcher.CurrentDispatcher;
            ClientList = new ObservableCollection<ClientViewmodel>();
            _core = new Core();
            _core.PropertyChanged += (sender, args) => NotifyPropertyChanged(args.PropertyName);

            _core.NewClientEvent += pClient =>
            {
                //do nothing
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
                    if (client == null)
                    {
                        client = new ClientViewmodel(pClient) {ID = pClient.ID};
                        this.ClientList.Add(client);
                        NotifyPropertyChanged(nameof(ConnectedClientsString));
                    }
                    client.UpdateValues(pClient);
                });
            };
            _core.NewLogMessageEvent += delegate(string message)
            {
                dispatcher.Invoke(() =>
                {
                    if (message != null)
                    {
                        //max log limit 1000
                        if (LogMessages.Count > 1000)
                            LogMessages.Dequeue();

                        LogMessages.Enqueue($"[{DateTime.UtcNow:G}] {message}");
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

            QueuedWorkItems.CollectionChanged += (sender, args) => NotifyPropertyChanged(nameof(QueuedOperationsCount));
            ActiveWorkItems.CollectionChanged += (sender, args) => NotifyPropertyChanged(nameof(ActiveOperationsCount));
            NotifyPropertyChanged(String.Empty);
        }

        internal void GenerateWork(Object obj)
        {
            //generate work parameter combinations; hardcoded for now
            //needs to include the training filename as first parameteer!
            List<List<String>> parameterslist = new List<List<String>>()
            {
                new List<String>()
                    {
                        "aio_binary_s2e.py",
                        "aio_binary_s2e_nopath.py",
                        "aio_binary_s2e_noplanned.py",
                        "aio_binary_s2e_noplanned_nopath.py",
                        "aio_binary_s2e_rgb.py",
                        "aio_binary_s2e_rgb_noplanned.py",
                        "aio_binary_s2s.py",
                        "aio_binary_s2s_nopath.py",
                        "aio_binary_s2s_noplanned.py",
                        "aio_binary_s2s_noplanned_nopath.py",
                        "aio_binary_s2s_rgb.py",
                        "aio_binary_s2s_rgb_noplanned.py",
                        "aio_numeric_s2e.py",
                        "aio_numeric_s2e_nopath.py",
                        "aio_numeric_s2e_noplanned.py",
                        "aio_numeric_s2e_noplanned_nopath.py",
                        "aio_numeric_s2e_rgb.py",
                        "aio_numeric_s2e_rgb_noplanned.py",
                        "aio_numeric_s2s.py",
                        "aio_numeric_s2s_nopath.py",
                        "aio_numeric_s2s_noplanned.py",
                        "aio_numeric_s2s_noplanned_nopath.py",
                        "aio_numeric_s2s_rgb.py",
                        "aio_numeric_s2s_rgb_noplanned.py"
                    },
                new List<String>()
                    {"1","2","3","4","5","6","7","8","9","10"},
                new List<String>()
                    {"100" },
                new List<String>()
                    {"0.1" },
                new List<String>()
                    {"20" },
                new List<String>()
                    {"1" },
            };
            
            _core.GenerateWorkUnits(parameterslist);
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
