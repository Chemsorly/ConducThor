using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConducThor_Server.Utility;

namespace ConducThor_Server.Commands
{
    class FilesystemManager : ManagerClass
    {
        public FilesystemManager()
        {
            
        }

        public void Initialize()
        {

            NotifyNewLogMessageEvent("Filesystem Manager initialized");
        }

        public bool CheckIfFileExists()
        {
            throw new NotImplementedException();
        }
    }
}
