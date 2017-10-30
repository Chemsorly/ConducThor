using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConducThor_Client.Machine;
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
        public delegate void NewConsoleMessage(String pMessage);
        public event LogEventHandler LogEvent;

        private ICommandManager _commandManager;
        private IFilesystemManager _filesystemManager;

        private Timer _pollTimer;
        private bool IsWorking = false;

        public SignalRManager(MachineData pMachineData)
        {
            _machineData = pMachineData;
            
            //create os specific managers manager based on OS type
            if (_machineData.OperatingSystem == OSEnum.Windows)
            {
                _commandManager = new WindowsCommandManager();
                _filesystemManager = new WindowsFilesystemManager();
            }
            else if (_machineData.OperatingSystem == OSEnum.Ubuntu)
            {
                _commandManager = new LinuxCommandManager();
                _filesystemManager = new LinuxFilesystemManager();
            }
            else
            {
                throw new Exception("undefined operating system");
            }

            //subscribe to events
            _commandManager.NewConsoleMessageEvent += delegate(string message) { LogEvent?.Invoke(message); };
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

            if(_pollTimer != null)
                _pollTimer.Dispose();

            _pollTimer = new Timer(delegate(object state)
            {
                if (IsWorking)
                    return;

                IsWorking = true;
                try
                {
                    var work = FetchWork();
                    if (work == null)
                        return;

                    LogEvent?.Invoke("Create process.");
                    var task = Task.Factory.StartNew(() =>
                    {
                        foreach (var command in work.Commands)
                            _commandManager.CreateProcess(command).Wait();
                    });
                    task.ContinueWith(t =>
                    {
                        IsWorking = false;
                        LogEvent?.Invoke("Process finished.");
                    });
                }
                catch (Exception e)
                {
                    IsWorking = false;
                    LogEvent?.Invoke(e.Message);
                }

            }, null, 6000, 60000);
        }

        private void Connect(){
            _hub = _connection.CreateHubProxy("CommHub");

            _connection.Start().Wait();
            LogEvent?.Invoke("Hub started");
            _hub.Invoke("Connect", _machineData);
            LogEvent?.Invoke("Connected to hub");
        }

        private WorkPackage FetchWork()
        {
            try
            {
                LogEvent?.Invoke("Fetch work.");
                var result = _hub.Invoke<WorkPackage>("FetchWork").Result;

                if (result != null)
                {
                    LogEvent?.Invoke($"Work package received: {String.Join("; ", result.Commands)}");
                }
                else
                    LogEvent?.Invoke($"Requested work package. None available.");
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Fetch work failed: {e.Message}");
                throw;
            }
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
