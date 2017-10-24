using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using ConducThor_Shared;
using Microsoft.AspNet.SignalR.Client;
using ConducThor_Shared.Connection;
using ConducThor_Shared.Enums;

namespace ConducThor_Client.Client
{
    class SignalRManager : INotifyPropertyChanged
    {
        public ConnectionState ConnectionState => _connection.State;

        private IHubProxy _hub;
        private Microsoft.AspNet.SignalR.Client.HubConnection _connection;
        private MachineData _machineData;

        public delegate void LogEventHandler(String message);
        public event LogEventHandler LogEvent;

        public SignalRManager(MachineData pMachineData)
        {
            _machineData = pMachineData;
        }

        public void Initialize(String pEndpoint)
        {
            _connection = new HubConnection(pEndpoint);
            _connection.Received += Connection_Received;
            _connection.StateChanged += delegate(StateChange obj){OnPropertyChanged(nameof(ConnectionState)); LogEvent?.Invoke($"state changed from {obj.OldState} to {obj.NewState}"); };
            _connection.Received += delegate(String s) { LogEvent?.Invoke(s); };
            _connection.Error += delegate(Exception ex) { LogEvent?.Invoke(ex.Message); };
            _connection.Reconnecting += delegate() { LogEvent?.Invoke("reconnecting"); };
            _connection.Reconnected += delegate { _hub.Invoke("Connect", _machineData); LogEvent?.Invoke("reconnected"); };
            _connection.Closed += delegate () {
                LogEvent?.Invoke("closed... restarting...");
                Initialize(pEndpoint);
            };
            Connect();
        }

        private void Connect(){
            _hub = _connection.CreateHubProxy("CommHub");

            _connection.Start().Wait();
            LogEvent?.Invoke("Hub started");
            _hub.Invoke("Connect", _machineData);
            LogEvent?.Invoke("Connected to hub");
        }

        private void Connection_Received(string obj)
        {
            LogEvent?.Invoke($"Received message from hub: {obj}");
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
