using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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

        private Timer _pollTimer;
        private bool IsWorking = false;
        private readonly ClientStatus _clientStatus = new ClientStatus();

        private ConcurrentQueue<String> SavedLog = new ConcurrentQueue<string>();

        public SignalRManager(MachineData pMachineData)
        {
            _machineData = pMachineData;
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

                    //init client status
                    _clientStatus.IsWorking = true;
                    _clientStatus.CurrentEpoch = 0;
                    _clientStatus.LastEpochDuration = "none";
                    _clientStatus.CurrentWorkParameters = work.Commands.First().Parameters;
                    SendStatusUpdate(_clientStatus);

                    //create log

                    //run process
                    NotifyLogMessageEvent("Create process.");
                    DateTime startTime = DateTime.UtcNow;
                    foreach (var command in work.Commands)
                    {
                        NotifyLogMessageEvent($"[DEBUG] Create process for: {command.FileName} {command.Arguments} {command.WorkDir}");
                        var startInfo = new ProcessStartInfo()
                        {
                            FileName = command.FileName,
                            WorkingDirectory = command.WorkDir,
                            Arguments = command.Arguments,
                            RedirectStandardOutput = true,
                            RedirectStandardInput = false,
                            RedirectStandardError = true,
                            UseShellExecute = false
                        };
                        using (var process = Process.Start(startInfo))
                        {
                            process.OutputDataReceived += (sender, args) => NotifyLogMessageEvent(args.Data);
                            process.ErrorDataReceived += (sender, args) => NotifyLogMessageEvent(args.Data);
                            NotifyLogMessageEvent($"[DEBUG] Starting process for: {command.FileName} {command.Arguments} {command.WorkDir}");
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();
                            process.WaitForExit();
                            NotifyLogMessageEvent($"[DEBUG] Finished process for: {command.FileName} {command.Arguments} {command.WorkDir}");
                        }
                    }
                    NotifyLogMessageEvent("Process finished.");

                    //get results
                    NotifyLogMessageEvent($"[DEBUG] Read target files");
                    List<ResultPackage.File> resultFiles = new List<ResultPackage.File>();
                    foreach (var pathlist in work.TargetFiles)
                    {
                        var path = System.IO.Path.Combine(pathlist.ToArray());
                        var filename = System.IO.Path.GetFileName(path);
                        var bytes = System.IO.File.ReadAllBytes(path);

                        resultFiles.Add(new ResultPackage.File()
                        {
                            FileData = bytes,
                            Filename = filename
                        });
                    }
                    SendResults(new ResultPackage()
                    {
                        WorkPackage = work,
                        DurationTime = DateTime.UtcNow - startTime,
                        ClientStatusAtEnd = _clientStatus,
                        ResultFiles = resultFiles,
                        MachineData = _machineData,
                        OutLog = FlushLog(this.SavedLog)
                    });
                    NotifyLogMessageEvent($"[DEBUG] Finished reading file modelfile");

                }
                catch (Exception e)
                {
                    IsWorking = false;
                    _clientStatus.IsWorking = false;
                    SendStatusUpdate(_clientStatus);
                    NotifyLogMessageEvent(e.Message);
                }
                finally
                {
                    IsWorking = false;
                    _clientStatus.IsWorking = false;
                    SendStatusUpdate(_clientStatus);
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
                NotifyLogMessageEvent("Fetch work.");
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
                return null;
            }
        }

        private void SendResults(ResultPackage pResults)
        {
            if (pResults == null)
                return;

            try
            {
                NotifyLogMessageEvent($"Send results");
                _hub.Invoke("SendResults", pResults).ContinueWith(t => NotifyLogMessageEvent($"Results sent."), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
            catch (Exception e)
            {
                NotifyLogMessageEvent($"SendResults failed: {e.Message}");
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
                //add to saved log
                SavedLog.Enqueue(pLogMessage);

                //check for special messages
                CheckForStatusMessages(pLogMessage);

                LogEvent?.Invoke(pLogMessage);
                SendConsoleMessage($"{pLogMessage}");
            }
        }
        private void CheckForStatusMessages(String pMessage)
        {
            //check for epoch match
            var epochmatch = Regex.Matches(pMessage, @"Epoch \d+\/\d+");
            if (epochmatch.Count > 0)
            {
                //get match
                var match = epochmatch[0].Value;

                //get first numbers
                var matches = Regex.Matches(match, @"\d+");

                //set value
                int value;
                if (int.TryParse(matches[0].Value, out value))
                {
                    _clientStatus.CurrentEpoch = value;
                    SendStatusUpdate(_clientStatus);
                }
                
            }

            //check for duration match
            var durationmatch = Regex.Matches(pMessage, @"\d+s");
            if (durationmatch.Count > 0)
            {
                _clientStatus.LastEpochDuration = durationmatch[0].Value;
                SendStatusUpdate(_clientStatus);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region server functions

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

            }
        }

        private void SendStatusUpdate(ClientStatus pClientStatus)
        {
            try
            {
                _hub.Invoke("UpdateStatus", pClientStatus);
            }
            catch (Exception ex)
            {

            }
        }

        private static List<String> FlushLog(ConcurrentQueue<String> pLog)
        {
            List<String> outlog = new List<string>();
            while (!pLog.IsEmpty)
            {
                String item;
                pLog.TryDequeue(out item);
                outlog.Add(item);
            }
            return outlog;
        }
        #endregion


    }
}
