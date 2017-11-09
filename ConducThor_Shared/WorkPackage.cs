using System;
using System.Collections.Generic;
using System.Text;

namespace ConducThor_Shared
{
    public class WorkPackage
    {
        public List<Command> Commands { get; set; }

        public class Command
        {
            public String FileName { get; set; }
            public String Arguments { get; set; }
            public String WorkDir { get; set; }
            public String Parameters { get; set; }
        }
    }
}
