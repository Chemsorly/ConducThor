using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConducThor_Shared;
using ConducThor_Shared.Enums;

namespace ConducThor_Server.Commands
{
    class CommandManager
    {
        public event Core.NewLogMessage NewLogMessageEvent;

        private ConcurrentQueue<String> _workParameters;

        public void Initialize()
        {
            _workParameters = new ConcurrentQueue<string>();

            NotifyNewLogMessageEvent("Command Manager initialized.");
        }

        public void CreateWorkParameters(List<List<double>> pCombinations)
        {
            NotifyNewLogMessageEvent("Create new set of work parameters.");
            var combinations = CombinationHelper.AllCombinationsOf(pCombinations.ToArray());

            //clear queue
            String ignored;
            while (!_workParameters.IsEmpty)
                _workParameters.TryDequeue(out ignored);

            //fill up queue
            foreach (var comb in combinations)
                _workParameters.Enqueue(String.Join(" ", comb));

            NotifyNewLogMessageEvent($"Finished creating new set of work parameters. Total: {_workParameters.Count}");
        }

        public WorkPackage CreateWorkPackage(OSEnum pOS)
        {
            if (pOS == OSEnum.Ubuntu)
                return new WorkPackage()
                {
                    //example ubuntu commands
                    Commands = new List<WorkPackage.Command>()
                    {
                        new WorkPackage.Command()
                        {
                            FileName = "/bin/bash",
                            Arguments = "-c \"git clone https://git.chemsorly.com/Chemsorly/MA-C2K-LSTM.git\"",
                            WorkDir = "/root/app"
                        },
                        new WorkPackage.Command()
                        {
                            FileName = "/bin/bash",
                            Arguments = "-c \"source /cntk/activate-cntk && /root/anaconda3/envs/cntk-py27/bin/python train_c2k.py\"",
                            WorkDir = "/root/app/MA-C2K-LSTM/code"
                        }
                    }
                };
            if (pOS == OSEnum.Windows)
                return new WorkPackage()
                {
                    //example windows commands
                    Commands = new List<WorkPackage.Command>()
                    {
                        new WorkPackage.Command()
                        {
                            FileName = "cmd",
                            Arguments = "/C \"git clone https://git.chemsorly.com/Chemsorly/MA-C2K-LSTM.git\"",
                            WorkDir = "C:\\app\\"
                        },
                        new WorkPackage.Command()
                        {
                            FileName = "python",
                            Arguments = "train_c2k.py",
                            WorkDir = "C:\\app\\MA-C2K-LSTM\\code"
                        }
                    }
                };
            return null;
        }

        private void NotifyNewLogMessageEvent(String pMessage)
        {
            NewLogMessageEvent?.Invoke(pMessage);
        }
    }
}
