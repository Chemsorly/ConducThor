using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ConducThor_Server.Annotations;
using ConducThor_Shared.Enums;

namespace ConducThor_Server.Model
{
    public class ClientViewmodel : INotifyPropertyChanged
    {
        private String _id = String.Empty;
        private String _machineName = String.Empty;
        private String _containerVersion = String.Empty;
        private OSEnum? _operatingSystem = null;
        private ProcessingUnitEnum? _processingUnit = null;

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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
