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
    [TokenAuthorizationAttribute]
    public class CommHub : Hub<IClient>
    {
        static IHubContext<IClient> _context = null;
        public delegate void NewClient(String pClientID);
        public delegate void ClientDisconnected(String pClientID);
        public delegate void MachineDataReceived(String pClientID, MachineData pMachineData);
        public delegate void NewClientLogMessage(String pClientID, String pLogMessage);
        public delegate void ClientStatusUpdated(String pClientID, ClientStatus pClientStatus);

        public static event NewClient NewClientEvent;
        public static event ClientDisconnected ClientDisconnectedEvent;
        public static event MachineDataReceived MachineDataReceivedEvent;
        public static event NewClientLogMessage NewClientLogMessageEvent;
        public static event Core.NewLogMessage NewLogMessageEvent;
        public static event ClientStatusUpdated ClientStatusUpdatedEvent;
        public static event SignalRManager.WorkRequested WorkRequestedEvent;
        public static event SignalRManager.ResultsReceived ResultsReceivedEvent;

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
            var work = WorkRequestedEvent?.Invoke(pMachineData.OperatingSystem, this.Context.ConnectionId);
            return work;
        }

        public void SendResults(ResultPackage pResults)
        {
            //debug
            NewLogMessageEvent?.Invoke($"Result files received from {this.Context.ConnectionId} with {pResults.ResultFiles.Sum(t => t.FileData.Length)} bytes in {pResults.ResultFiles.Count} files");
            ResultsReceivedEvent?.Invoke(pResults, this.Context.ConnectionId);
        }

        public void UpdateStatus(ClientStatus pStatus)
        {
            ClientStatusUpdatedEvent?.Invoke(this.Context.ConnectionId, pStatus);
        }

        public void SendConsoleMessage(String pMessage)
        {
            NewClientLogMessageEvent?.Invoke(this.Context.ConnectionId,pMessage);
        }

    }
}
