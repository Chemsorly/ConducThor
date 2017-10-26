﻿using System;
using System.Collections.Concurrent;
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

        public delegate void NewClient(Client pClientID);
        public delegate void ClientUpdated(Client pClient);
        public delegate void ClientDisconnected(Client pClientID);

        List<Client> Clients = new List<Client>();

        public event ClientUpdated ClientUpdatedEvent;
        public event NewClient NewClientEvent;
        public event ClientDisconnected ClientDisconnectedEvent;

        public void Initialize()
        {
            CommHub.NewClientEvent += NotifyNewClientEvent;
            CommHub.ClientDisconnectedEvent += NotifyClientDisconnectedEvent;
            CommHub.MachineDataReceivedEvent += (id, data) =>
            {
                var client = Clients.FirstOrDefault(t => t.ID == id);
                if (client != null)
                {
                    client.ContainerVersion = data.ContainerVersion;
                    client.OperatingSystem = data.OperatingSystem;
                    client.ProcessingUnit = data.ProcessingUnit;
                    NotifyClientUpdatedEvent(client);
                }
            };

            try
            {
                _signalrapp = WebApp.Start<SignalRStartup>("http://*:8080/");
            }
            catch (Exception ex)
            {
                
            }
        }

        private void NotifyNewClientEvent(String pClientID)
        {
            lock (Clients)
            {
                //add client if not exist
                if (Clients.All(t => t.ID != pClientID))
                    Clients.Add(new Client()
                    {
                        ID = pClientID
                    });

                NewClientEvent?.Invoke(Clients.First(t => t.ID == pClientID));
            }
        }

        private void NotifyClientDisconnectedEvent(String pClientID)
        {
            lock (Clients)
            {
                var client = Clients.FirstOrDefault(t => t.ID == pClientID);
                if (client != null)
                {
                    Clients.Remove(client);
                    ClientDisconnectedEvent?.Invoke(client);
                }
            }
        }

        private void NotifyClientUpdatedEvent(Client pClient)
        {
            ClientUpdatedEvent?.Invoke(pClient);
        }
    }
}