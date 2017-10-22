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

        public static event SignalRManager.NewClient NewClientEvent;
        public static event SignalRManager.ClientUpdated ClientUpdatedEvent;

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

        /// <summary>
        /// answer function for Ping
        /// </summary>
        public void Pong(ClientStatus pStatus)
        {
            Console.WriteLine("PONG");
        }

        public void Connect()
        {
            NotifyNewClientEvent(new ClientViewmodel() {ID = "test", Status = true});
        }

        public void NotifyNewClientEvent(ClientViewmodel pClient)
        {
            NewClientEvent?.Invoke(pClient);
        }

        public void NotifyClientUpdatedEvent(ClientViewmodel pClient)
        {
            ClientUpdatedEvent?.Invoke(pClient);
        }
    }
}
