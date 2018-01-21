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
        public List<File> ResultFiles { get; set; }
        public List<String> OutLog { get; set; }

        public class File
        {
            public String Filename { get; set; }
            public byte [] FileData { get; set; }
        }
    }



}
