using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ConducThor_Server.Model;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;

namespace ConducThor_Server.Server
{
    public class SignalRManager
    {
        private IDisposable _signalrapp;

        public delegate void NewClient(ClientViewmodel pClient);
        public event NewClient NewClientEvent;

        public delegate void ClientUpdated(ClientViewmodel pClient);
        public event ClientUpdated ClientUpdatedEvent;

        private System.Timers.Timer _testtimer;

        public void Initialize()
        {
            CommHub.NewClientEvent += NotifyNewClientEvent;
            CommHub.ClientUpdatedEvent += NotifyClientUpdatedEvent;

            try
            {
                _signalrapp = WebApp.Start<SignalRStartup>("http://*:8080/");
            }
            catch (Exception ex)
            {
                
            }

            _testtimer = new System.Timers.Timer();
            _testtimer.Interval = 10000;
            _testtimer.AutoReset = true;
            _testtimer.Elapsed += delegate(object sender, ElapsedEventArgs args)
                {
                    CommHub.HubContext.Clients.All.Ping();
                };
            _testtimer.Start();

            //NotifyNewClientEvent(new ClientViewmodel() {ID = "subtest"});
        }

        private void NotifyNewClientEvent(ClientViewmodel pClient)
        {
            NewClientEvent?.Invoke(pClient);
        }

        private void NotifyClientUpdatedEvent(ClientViewmodel pClient)
        {
            ClientUpdatedEvent?.Invoke(pClient);
        }
    }
}
