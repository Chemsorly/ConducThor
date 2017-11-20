using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConducThor_Shared;
using ConducThor_Shared.Enums;

namespace ConducThor_Server.Commands
{
    public class WorkItem
    {
        public delegate void OnTimeoutHappened(WorkItem sender);

        public event OnTimeoutHappened OnTimeoutHappenedEvent;


        public WorkPackage WorkPackage { get; private set; }
        public String Parameters { get; }
        public String ClientID { get; private set; }
        public DateTime StartDate { get; private set; }

        private System.Timers.Timer _timeoutTimer;
        private const int TimeoutValue = 86400000; //86400s = 24h; those are ms

        public WorkItem(String pParameters)
        {
            //specify default values
            StartDate = DateTime.MinValue;
            Parameters = pParameters;
        }

        public WorkPackage Start(OSEnum pOS, String pAssignedClient)
        {
            StartDate = DateTime.UtcNow;
            ClientID = pAssignedClient;

            _timeoutTimer = new System.Timers.Timer();
            _timeoutTimer.Interval = TimeoutValue;
            _timeoutTimer.Elapsed += (sender, args) =>
            {
                OnTimeoutHappenedEvent?.Invoke(this);
                StartDate = DateTime.MinValue;
                ClientID = String.Empty;
                WorkPackage = null;
                _timeoutTimer = null;
            };
            _timeoutTimer.Start();

            WorkPackage = CreateWorkPackage(pOS, Parameters);
            return WorkPackage;
        }

        private WorkPackage CreateWorkPackage(OSEnum pOS, String pParameter)
        {
            //clean
            var paras = pParameter.Replace(",", ".");

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
                            WorkDir = "/root/app",
                            Parameters = paras
                        },
                        new WorkPackage.Command()
                        {
                            FileName = "/bin/bash",
                            Arguments = $"-c \"source /cntk/activate-cntk && /root/anaconda3/envs/cntk-py27/bin/python -u train_c2k_experiment.py {paras}\"",
                            WorkDir = "/root/app/MA-C2K-LSTM/code",
                            Parameters = paras
                        },
                        new WorkPackage.Command()
                        {
                            FileName = "/bin/bash",
                            Arguments = $"-c \"source /cntk/activate-cntk && /root/anaconda3/envs/cntk-py27/bin/python -u make_predictions_c2k.py\"",
                            WorkDir = "/root/app/MA-C2K-LSTM/code",
                            Parameters = paras
                        },
                    },
                    TargetModelFile = new List<string>() { "MA-C2K-LSTM","code", "output_files", "models", "model-latest.h5"},
                    TargetPredictionFile = new List<string>() { "MA-C2K-LSTM","code", "output_files", "results", "results.csv"}
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
                            WorkDir = "C:\\app\\",
                            Parameters = paras
                        },
                        new WorkPackage.Command()
                        {
                            FileName = "python",
                            Arguments = $"-u train_c2k_experiment.py {paras}",
                            WorkDir = "C:\\app\\MA-C2K-LSTM\\code",
                            Parameters = paras
                        },
                        new WorkPackage.Command()
                        {
                            FileName = "python",
                            Arguments = $"-u make_predictions_c2k.py",
                            WorkDir = "C:\\app\\MA-C2K-LSTM\\code",
                            Parameters = paras
                        }
                    },
                    TargetModelFile = new List<string>() { "MA-C2K-LSTM", "code", "output_files", "models", "model-latest.h5" },
                    TargetPredictionFile = new List<string>() { "MA-C2K-LSTM", "code", "output_files", "results", "results.csv" }
                };
            return null;
        }
    }
}
