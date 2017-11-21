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

        public override void Initialize()
        {
            _filesystemManager = new FilesystemManager();
            _filesystemManager.NewLogMessageEvent += NotifyNewLogMessageEvent;
            _filesystemManager.Initialize();
            base.Initialize();
        }

        public bool CheckIfResultExists(String pWorkparameters)
        {
            return _filesystemManager.CheckIfFileExists(pWorkparameters);
        }

        public bool VerifyAndSave(ResultPackage pResults)
        {
            //check for integrity (aka byte size)
            if (pResults.ModelFile.Length < 700000 || pResults.PredictionFile.Length < 1500000)
            {
                NotifyNewLogMessageEvent($"[ERROR] Received invalid results for {pResults.WorkPackage.Commands.First().Parameters}: Model File or Result File too small");
            }

            //check if already exist
            if (_filesystemManager.CheckIfFileExists(pResults.WorkPackage.Commands.First().Parameters))
            {
                NotifyNewLogMessageEvent($"[ERROR] Attempting to write duplicate file for {pResults.WorkPackage.Commands.First().Parameters}");
                return false;
            }

            try
            {
                _filesystemManager.WriteResultsToFilesystem(pResults);
            }
            catch (Exception ex)
            {
                NotifyNewLogMessageEvent($"[ERROR] {ex.Message}");
                return false;
            }
            return true;
        }
    }
}
