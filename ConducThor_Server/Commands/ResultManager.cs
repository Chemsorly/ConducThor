using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConducThor_Server.Utility;
using ConducThor_Shared;

namespace ConducThor_Server.Commands
{
    class ResultManager : ManagerClass
    {
        private FilesystemManager _filesystemManager;

        public ResultManager()
        {
            _filesystemManager = new FilesystemManager();
            _filesystemManager.NewLogMessageEvent += NotifyNewLogMessageEvent;
        }

        public override void Initialize()
        {

            _filesystemManager.Initialize();
            base.Initialize();
        }

        public bool CheckIfResultExists()
        {
            throw new NotImplementedException();
        }

        public bool VerifyAndSave(ResultPackage pResults)
        {
            throw new NotImplementedException();
        }
    }
}
