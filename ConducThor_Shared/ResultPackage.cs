using System;
using System.Collections.Generic;
using System.Text;

namespace ConducThor_Shared
{
    public class ResultPackage
    {
        public WorkPackage WorkPackage { get; set; }
        public TimeSpan DurationTime { get; set; }
        public ClientStatus ClientStatusAtEnd { get; set; }
        public MachineData MachineData { get; set; }
        public byte[] ModelFile { get; set; }
        public byte[] PredictionFile { get; set; }
    }
}
