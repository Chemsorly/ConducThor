using System;
using System.Collections.Generic;
using System.Text;

namespace ConducThor_Shared
{
    public class WorkPackage
    {
        public List<Command> Commands { get; set; }

        /// <summary>
        /// List(List(String)) because Windows and linux use different file system delimiters, therfor a filepath is stored as List(String) and reconstructed adhoc
        /// </summary>
        public List<List<String>> TargetFiles { get; set; }

        public class Command
        {
            public String FileName { get; set; }
            public String Arguments { get; set; }
            public String WorkDir { get; set; }
            public String Parameters { get; set; }
        }
    }
}
