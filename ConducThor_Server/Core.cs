using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ConducThor_Server.Annotations;
using ConducThor_Server.Commands;
using ConducThor_Server.Model;
using ConducThor_Server.Server;
using ConducThor_Server.Utility;
using ConducThor_Shared;
using ConducThor_Shared.Enums;

namespace ConducThor_Server
{
    public class Core : ManagerClass, INotifyPropertyChanged
    {
        private SignalRManager _signalrmanager;
        private UpdateNotifier _updateNotifier;
        private CommandManager _commandManager;

        //forwarded events from SignalR manager
        public event SignalRManager.ClientUpdated ClientUpdatedEvent;
        public event SignalRManager.NewClient NewClientEvent;
        public event SignalRManager.ClientDisconnected ClientDisconnectedEvent;
        public event SignalRManager.NewClientLogMessage NewConsoleLogMessage;

        public event PropertyChangedEventHandler PropertyChanged;

        //public String VersionStatus => _updateNotifier == null ? String.Empty : (_updateNotifier.Status == Utility.VersionStatus.UpdateAvailable ? " Update available!" : String.Empty);
        public AsyncObservableCollection<WorkItem> QueuedWorkItems => _commandManager.QueuedWorkItems;
        public AsyncObservableCollection<WorkItem> ActiveWorkItems => _commandManager.ActiveWorkItems;

        public override void Initialize()
        {
            //init updater
            //_updateNotifier = new UpdateNotifier();
            //_updateNotifier.PropertyChanged += (sender, args) => OnPropertyChanged(nameof(VersionStatus));

            //forward signalr manager
            _signalrmanager = new SignalRManager();
            _signalrmanager.NewClientEvent +=delegate(Client client) { NewClientEvent?.Invoke(client);  };
            _signalrmanager.ClientDisconnectedEvent += delegate(Client client) { ClientDisconnectedEvent?.Invoke(client); };
            _signalrmanager.ClientUpdatedEvent += delegate(Client client) { ClientUpdatedEvent?.Invoke(client); };
            _signalrmanager.NewLogMessageEvent += NotifyNewLogMessageEvent;
            _signalrmanager.NewConsoleLogMessage += delegate (Client pClient, string message) { NewConsoleLogMessage?.Invoke(pClient,message); };
            _signalrmanager.WorkRequestedEvent += SignalrmanagerOnWorkRequestedEvent;
            _signalrmanager.ResultsReceivedEvent += SignalrmanagerOnResultsReceivedEvent;
            _signalrmanager.Initialize();

            //create command manager
            _commandManager = new CommandManager();
            _commandManager.NewLogMessageEvent += NotifyNewLogMessageEvent;
            _commandManager.Initialize();

            base.Initialize();
        }

        private void SignalrmanagerOnResultsReceivedEvent(ResultPackage pResults, string pClientID)
        {
            _commandManager.SendResults(pResults);
        }

        public void GenerateWorkUnits(List<List<double>> pWorkParameters)
        {
            _commandManager.CreateWorkParameters(pWorkParameters);
        }

        private WorkPackage SignalrmanagerOnWorkRequestedEvent(OSEnum pos, string pclientid)
        {
            return _commandManager.FetchWork(pos, pclientid);
        }


        #region INotifyPropertyChanged
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
