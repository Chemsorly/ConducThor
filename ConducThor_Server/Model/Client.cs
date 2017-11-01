using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConducThor_Server.Utility;
using ConducThor_Shared.Enums;

namespace ConducThor_Server.Model
{
    public class Client
    {
        public String ID { get; set; }
        public String MachineName { get; set; }
        public String ContainerVersion { get; set; }
        public OSEnum OperatingSystem { get; set; }
        public ProcessingUnitEnum ProcessingUnit { get; set; }
        public AsyncObservableCollection<String> LogMessages { get; set; }
    }
}
