using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNet.SignalR.Client;

namespace ConducThor_Client.Client
{
    class SignalRManager : INotifyPropertyChanged
    {
        public ConnectionState ConnectionState => _connection.State;

        private Microsoft.AspNet.SignalR.Client.HubConnection _connection;

        public SignalRManager()
        {
            
        }

        public void Initialize(String pEndpoint)
        {
            _connection = new HubConnection(pEndpoint);

            _connection.Received += Connection_Received;
            _connection.StateChanged += delegate(StateChange obj){OnPropertyChanged(nameof(ConnectionState)); LogEvent?.Invoke($"state changed from {obj.OldState} to {obj.NewState}"); };
            _connection.Received += delegate(String s) { LogEvent?.Invoke(s); };
            _connection.Error += delegate(Exception ex) { LogEvent?.Invoke(ex.Message); };
            _connection.Reconnecting += delegate() { LogEvent?.Invoke("reconnecting"); };
            _connection.Closed += delegate () { LogEvent?.Invoke("closed"); };
            _connection.Start().Wait();

            LogEvent?.Invoke("hub started");
        }

        private void Connection_Received(string obj)
        {
            LogEvent?.Invoke($"Received message from hub: {obj}");
        }

        public delegate void LogEventHandler(String message);
        public event LogEventHandler LogEvent;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
