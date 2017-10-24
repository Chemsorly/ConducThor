using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ConducThor_Server.Model;
using ConducThor_Shared;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;

namespace ConducThor_Server.Server
{
    public class SignalRManager
    {
        private IDisposable _signalrapp;

        public delegate void NewClient(String pClientID);
        public delegate void ClientUpdated(Client pClient);
        public delegate void ClientDisconnected(String pClientID);

        public event ClientUpdated ClientUpdatedEvent;
        public event NewClient NewClientEvent;
        public event ClientDisconnected ClientDisconnectedEvent;

        public void Initialize()
        {
            CommHub.NewClientEvent += NotifyNewClientEvent;
            CommHub.ClientUpdatedEvent += NotifyClientUpdatedEvent;
            CommHub.ClientDisconnectedEvent += id => ClientDisconnectedEvent?.Invoke(id);
            CommHub.MachineDataReceivedEvent += CommHubOnMachineDataReceivedEvent;

            try
            {
                _signalrapp = WebApp.Start<SignalRStartup>("http://*:8080/");
            }
            catch (Exception ex)
            {
                
            }
        }

        private void CommHubOnMachineDataReceivedEvent(string pClientId, MachineData pMachineData)
        {
            
        }

        private void NotifyNewClientEvent(String pClientID)
        {
            NewClientEvent?.Invoke(pClientID);
        }

        private void NotifyClientUpdatedEvent(Client pClient)
        {
            ClientUpdatedEvent?.Invoke(pClient);
        }
    }
}
