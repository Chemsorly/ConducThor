using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
                NotifyLogMessageEvent("WindowsOS managers initialized");
            }
            else if (_machineData.OperatingSystem == OSEnum.Ubuntu)
            {
                _commandManager = new LinuxCommandManager();
                _filesystemManager = new LinuxFilesystemManager();
                NotifyLogMessageEvent("LinuxOS managers initialized");
            }
            else
            {
                throw new Exception("undefined operating system");
            }

            //subscribe to events
            _commandManager.NewConsoleMessageEvent += delegate(string message)
            {
                NotifyLogMessageEvent(message);
                SendConsoleMessage(message);
            };
        }

        public void Initialize(String pEndpoint)
        {
            _connection = new HubConnection(pEndpoint);
            _connection.Received += Connection_Received;
            _connection.StateChanged += delegate(StateChange obj){OnPropertyChanged(nameof(ConnectionState)); NotifyLogMessageEvent($"state changed from {obj.OldState} to {obj.NewState}"); };
            _connection.Received += delegate(String s) { NotifyLogMessageEvent(s); };
            _connection.Error += delegate(Exception ex) { NotifyLogMessageEvent(ex.Message); };
            _connection.Reconnecting += delegate() { NotifyLogMessageEvent("reconnecting"); };
            _connection.Reconnected += delegate { _hub.Invoke("Connect", _machineData); NotifyLogMessageEvent("reconnected"); };
            _connection.Closed += delegate () {
                NotifyLogMessageEvent("closed... restarting...");
                Initialize(pEndpoint);
            };
            Connect();

            _pollTimer?.Dispose();
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

                    NotifyLogMessageEvent("Create process.");
                    foreach (var command in work.Commands)
                    {
                        NotifyLogMessageEvent($"[DEBUG] Create process for: {command.FileName} {command.Arguments} {command.WorkDir}");
                        Task.Factory.StartNew(()=>
                        {
                            var StartInfo = new ProcessStartInfo()
                            {
                                FileName = command.FileName,
                                WorkingDirectory = command.WorkDir,
                                Arguments = command.Arguments,
                                RedirectStandardOutput = true,
                                RedirectStandardInput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false
                            };
                            var process = new Process { StartInfo = StartInfo };
                            process.OutputDataReceived += (sender, args) => NotifyLogMessageEvent(args.Data);
                            process.ErrorDataReceived += (sender, args) => NotifyLogMessageEvent(args.Data);
                            NotifyLogMessageEvent($"[DEBUG] Starting process for: {command.FileName} {command.Arguments} {command.WorkDir}");
                            process.Start();
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();
                            process.WaitForExit();
                            NotifyLogMessageEvent($"[DEBUG] Finished process for: {command.FileName} {command.Arguments} {command.WorkDir}");
                            process.Dispose();
                        }).Wait();
                    }
                    NotifyLogMessageEvent("Process finished.");
                }
                catch (Exception e)
                {
                    IsWorking = false;
                    NotifyLogMessageEvent(e.Message);
                }
                finally
                {
                    IsWorking = false;
                }

            }, null, 6000, 60000);
        }

        private void Connect(){
            _hub = _connection.CreateHubProxy("CommHub");
            _connection.Start().Wait();
            NotifyLogMessageEvent("Hub started");
            _hub.Invoke("Connect", _machineData);
            NotifyLogMessageEvent("Connected to hub");
        }

        private WorkPackage FetchWork()
        {
            try
            {
                LogEvent?.Invoke("Fetch work.");
                var result = _hub.Invoke<WorkPackage>("FetchWork", _machineData).Result;

                if (result != null)
                {
                    NotifyLogMessageEvent($"Work package received from server: {String.Join("; ", result.Commands.Select(t => $"{t.FileName} {t.Arguments} {t.WorkDir}"))}");
                }
                else
                    NotifyLogMessageEvent($"Requested work package. None available.");
                return result;
            }
            catch (Exception e)
            {
                NotifyLogMessageEvent($"Fetch work failed: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// sends a console message to the server
        /// </summary>
        /// <param name="pConsoleMessage"></param>
        private void SendConsoleMessage(String pConsoleMessage)
        {
            try
            {
                _hub.Invoke("SendConsoleMessage", pConsoleMessage);
            }
            catch (Exception ex)
            {
                //NotifyLogMessageEvent($"ERROR (SendConsoleMessage): {ex.Message}");
            }
        }

        private void Connection_Received(string obj)
        {
            NotifyLogMessageEvent($"Received message from hub: {obj}");
        }


        private void NotifyLogMessageEvent(String pLogMessage)
        {
            if (!String.IsNullOrWhiteSpace(pLogMessage))
            {
                LogEvent?.Invoke(pLogMessage);
                SendConsoleMessage($"{pLogMessage}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
