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

        //parameters
        private List<List<DataGridStringItem>> ParameterList { get; set; }
        public List<DataGridStringItem> Parameter0List { get; set; }
        public List<DataGridStringItem> Parameter1List { get; set; }
        public List<DataGridStringItem> Parameter2List { get; set; }
        public List<DataGridStringItem> Parameter3List { get; set; }
        public List<DataGridStringItem> Parameter4List { get; set; }
        public List<DataGridStringItem> Parameter5List { get; set; }

        //ui
        public ICommand GenerateWorkCommand { get; set; }


        public void Initialize()
        {
            #region plist
            //init plist
            ParameterList = new List<List<DataGridStringItem>>();
            Parameter0List = new List<DataGridStringItem>()
            {
                new DataGridStringItem() { Value = "aio_numeric_s2e_nopath_onlytimeloss.py"},
                new DataGridStringItem() { Value = "aio_numeric_s2e_noplanned_nopath_onlytimeloss.py"},
                new DataGridStringItem() { Value = "aio_numeric_s2e_noplanned_onlytimeloss.py"},
                new DataGridStringItem() { Value = "aio_numeric_s2e_onlytimeloss.py"},
                new DataGridStringItem() { Value = "aio_numeric_s2e_rgb_noplanned_onlytimeloss.py"},
                new DataGridStringItem() { Value = "aio_numeric_s2e_rgb_onlytimeloss.py"}
            };
            ParameterList.Add(Parameter0List);
            Parameter1List = new List<DataGridStringItem>();
            for(int i = 1; i <= 30; i++) Parameter1List.Add(new DataGridStringItem() { Value = i.ToString()});

            ParameterList.Add(Parameter1List);
            Parameter2List = new List<DataGridStringItem>() { new DataGridStringItem() { Value = "100" } };
            ParameterList.Add(Parameter2List);
            Parameter3List = new List<DataGridStringItem>() { new DataGridStringItem() { Value = "0.1" } };
            ParameterList.Add(Parameter3List);
            Parameter4List = new List<DataGridStringItem>() { new DataGridStringItem() { Value = "20" } };
            ParameterList.Add(Parameter4List);
            Parameter5List = new List<DataGridStringItem>() { new DataGridStringItem() { Value = "1" } };
            ParameterList.Add(Parameter5List);

            #endregion

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
            _core.GenerateWorkUnits(ParameterList.Select(t => t.Select(u => u.Value).ToList()).ToList());
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
              PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public class DataGridStringItem { public String Value { get; set; } }
    }
}
