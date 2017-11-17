using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ConducThor_Server.Model;
using ConducThor_Server.Utility;
using ConducThor_Shared;
using ConducThor_Shared.Enums;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;

namespace ConducThor_Server.Server
{
    public class SignalRManager : ManagerClass
    {
        private IDisposable _signalrapp;

        public delegate void NewClient(Client pClient);
        public delegate void ClientUpdated(Client pClient);
        public delegate void ClientDisconnected(Client pClient);
        public delegate void NewClientLogMessage(Client pClient, String pMessage);
        public delegate WorkPackage WorkRequested(OSEnum pOS, String pClientID);
        public delegate void ResultsReceived(ResultPackage pResults, String pClientID);

        readonly List<Client> _clients = new List<Client>();

        public event ClientUpdated ClientUpdatedEvent;
        public event NewClient NewClientEvent;
        public event ClientDisconnected ClientDisconnectedEvent;
        public event NewClientLogMessage NewConsoleLogMessage;
        public event WorkRequested WorkRequestedEvent;
        public event ResultsReceived ResultsReceivedEvent;

        public override void Initialize()
        {
            CommHub.NewClientEvent += NotifyNewClientEvent;
            CommHub.ClientDisconnectedEvent += NotifyClientDisconnectedEvent;
            CommHub.NewLogMessageEvent += NotifyNewLogMessageEvent;
            CommHub.NewClientLogMessageEvent += NewClientLogMessageEvent;
            CommHub.MachineDataReceivedEvent += (id, data) =>
            {
                var client = _clients.FirstOrDefault(t => t.ID == id);
                if (client != null)
                {
                    client.ContainerVersion = data.ContainerVersion;
                    client.OperatingSystem = data.OperatingSystem;
                    client.ProcessingUnit = data.ProcessingUnit;
                    NotifyClientUpdatedEvent(client);
                }
            };
            CommHub.ClientStatusUpdatedEvent += (id, status) =>
            {
                var client = _clients.FirstOrDefault(t => t.ID == id);
                if (client != null)
                {
                    client.IsWorking = status.IsWorking;
                    client.LastEpochDuration = status.LastEpochDuration;
                    if(status.CurrentEpoch > client.CurrentEpoch)
                        client.CurrentEpoch = status.CurrentEpoch;
                    client.CurrentWorkParameters = status.CurrentWorkParameters;
                    NotifyClientUpdatedEvent(client);
                }
            };
            CommHub.WorkRequestedEvent += (os, id) => WorkRequestedEvent?.Invoke(os, id);
            CommHub.ResultsReceivedEvent += (results, id) => ResultsReceivedEvent?.Invoke(results, id);

            try
            {
                _signalrapp = WebApp.Start<SignalRStartup>("http://*:8080/");
                NotifyNewLogMessageEvent("SignalR listener initialized to port 8080");
            }
            catch (Exception ex)
            {
                NotifyNewLogMessageEvent($"Could not start SignalR listener on port 8080. Are you running the application as admin? Exception: {ex.Message}");
            }

            base.Initialize();
        }

        private void NotifyNewClientEvent(String pClientID)
        {
            lock (_clients)
            {
                //add client if not exist
                if (_clients.All(t => t.ID != pClientID))
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _clients.Add(new Client()
                        {
                            ID = pClientID
                        });
                    });
                }
                NewClientEvent?.Invoke(_clients.First(t => t.ID == pClientID));
                NotifyNewLogMessageEvent($"Client connected: {pClientID}");
            }
        }

        private void NotifyClientDisconnectedEvent(String pClientID)
        {
            lock (_clients)
            {
                var client = _clients.FirstOrDefault(t => t.ID == pClientID);
                if (client != null)
                {
                    _clients.Remove(client);
                    ClientDisconnectedEvent?.Invoke(client);
                    NotifyNewLogMessageEvent($"Client disconnected: {pClientID}");
                }
            }
        }
        private void NotifyClientUpdatedEvent(Client pClient)
        {
            ClientUpdatedEvent?.Invoke(pClient);
            NotifyNewLogMessageEvent($"CONNECT: {pClient.ID}");
        }

        private void NewClientLogMessageEvent(string pClientID, string pLogMessage)
        {
            //discard empty messages
            if (String.IsNullOrWhiteSpace(pLogMessage))
                return;

            var targetClient = _clients.FirstOrDefault(t => t.ID == pClientID);
            if (targetClient != null)
            {
                NewConsoleLogMessage?.Invoke(targetClient, pLogMessage);
            }
        }
    }
}
