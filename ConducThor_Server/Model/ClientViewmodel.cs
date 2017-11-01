using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ConducThor_Server.Annotations;
using ConducThor_Server.Utility;
using ConducThor_Shared.Enums;

namespace ConducThor_Server.Model
{
    public class ClientViewmodel : INotifyPropertyChanged
    {
        private Client _client;
        private String _id = String.Empty;
        private String _machineName = String.Empty;
        private String _containerVersion = String.Empty;
        private OSEnum? _operatingSystem = null;
        private ProcessingUnitEnum? _processingUnit = null;

        public ClientViewmodel(Client pClient)
        {
            _client = pClient;
        }

        public String ID
        {
            get => _id;
            set { _id = value; OnPropertyChanged();}
        }
        public String MachineName
        {
            get => _machineName;
            set { _machineName = value; OnPropertyChanged(); }
        }
        public String ContainerVersion
        {
            get => _containerVersion;
            set { _containerVersion = value; OnPropertyChanged(); }
        }
        public OSEnum OperatingSystem
        {
            get => _operatingSystem ?? OSEnum.undefined;
            set { _operatingSystem = value; OnPropertyChanged(); }
        }
        public ProcessingUnitEnum ProcessingUnit
        {
            get => _processingUnit ?? ProcessingUnitEnum.undefined;
            set { _processingUnit = value; OnPropertyChanged(); }
        }

        public AsyncObservableCollection<String> LogMessages => _client.LogMessages;

        public void UpdateValues(Client pClient)
        {
            ContainerVersion = pClient.ContainerVersion;
            OperatingSystem = pClient.OperatingSystem;
            ProcessingUnit = pClient.ProcessingUnit;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
