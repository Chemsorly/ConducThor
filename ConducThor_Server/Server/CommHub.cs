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
using Microsoft.AspNet.SignalR;

namespace ConducThor_Server.Server
{
    public class CommHub : Hub<IClient>
    {
        static IHubContext<IClient> _context = null;
        public delegate void NewClient(String pClientID);
        public delegate void ClientDisconnected(String pClientID);
        public delegate void MachineDataReceived(String pClientID, MachineData pMachineData);

        public static event NewClient NewClientEvent;
        public static event ClientDisconnected ClientDisconnectedEvent;
        public static event MachineDataReceived MachineDataReceivedEvent;


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
    }
}
