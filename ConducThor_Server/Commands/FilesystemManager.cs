using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
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

        public override void Initialize()
        {

            base.Initialize();
        }

        public bool CheckIfFileExists()
        {
            throw new NotImplementedException();
        }
    }
}
