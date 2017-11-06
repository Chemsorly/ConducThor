using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ConducThor_Server.Model;
using ConducThor_Shared;
using ConducThor_Shared.Connection;
using ConducThor_Shared.Enums;
using Microsoft.AspNet.SignalR;

namespace ConducThor_Server.Server
{
    public class CommHub : Hub<IClient>
    {
        static IHubContext<IClient> _context = null;
        public delegate void NewClient(String pClientID);
        public delegate void ClientDisconnected(String pClientID);
        public delegate void MachineDataReceived(String pClientID, MachineData pMachineData);
        public delegate void NewClientLogMessage(String pClientID, String pLogMessage);

        public static event NewClient NewClientEvent;
        public static event ClientDisconnected ClientDisconnectedEvent;
        public static event MachineDataReceived MachineDataReceivedEvent;
        public static event NewClientLogMessage NewClientLogMessageEvent;
        public static event SignalRManager.NewLogMessage NewLogMessageEvent;

        /// <summary>
        /// Context instance to access client connections to broadcast to
        /// </summary>
        public static IHubContext<IClient> HubContext
        {
            get
            {
                if (_context == null)
                    _context = GlobalHost.ConnectionManager.GetHubContext<CommHub, IClient>();

                return _context;
            }
        }

        public override Task OnConnected()
        {
            NewClientEvent?.Invoke(this.Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            NewClientEvent?.Invoke(this.Context.ConnectionId);
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            ClientDisconnectedEvent?.Invoke(this.Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        public void Connect(MachineData pMachineData)
        {
            MachineDataReceivedEvent?.Invoke(this.Context.ConnectionId,pMachineData);
        }

        public WorkPackage FetchWork(MachineData pMachineData)
        {
            //debug test
            NewLogMessageEvent?.Invoke($"New Work Request received from {this.Context.ConnectionId}");

            if(pMachineData.OperatingSystem == OSEnum.Ubuntu)
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
            else if (pMachineData.OperatingSystem == OSEnum.Windows)
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
            else
            {
                return null;
            }
        }

        public void UpdateStatus(ClientStatus pStatus)
        {
            
        }

        public void SendConsoleMessage(String pMessage)
        {
            NewClientLogMessageEvent?.Invoke(this.Context.ConnectionId,pMessage);
        }

    }
}
