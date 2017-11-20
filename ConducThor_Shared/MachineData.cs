using System;
using System.Collections.Generic;
using System.Text;
using ConducThor_Shared.Enums;

namespace ConducThor_Shared
{
    public class MachineData
    {
        //public String MachineName { get; set; }
        public String ContainerVersion { get; set; }
        public OSEnum OperatingSystem { get; set; }
        public ProcessingUnitEnum ProcessingUnit { get; set; }
        public String Name { get; set; }
    }
}
