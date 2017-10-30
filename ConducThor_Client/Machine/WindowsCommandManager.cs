using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using ConducThor_Client.Client;
using ConducThor_Shared;

namespace ConducThor_Client.Machine
{
    class WindowsCommandManager : ICommandManager
    {
        public override Task CreateProcess(WorkPackage.Command pCommand)
        {
            return Task.Factory.StartNew(() =>
            {
                //https://loune.net/2017/06/running-shell-bash-commands-in-net-core/
                NotifyNewConsoleMessageEvent($"Create process with command: {pCommand}");
                using (var process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "cmd",
                        WorkingDirectory = @"C:\app",
                        Arguments = $"/C {pCommand}",
                        RedirectStandardOutput = true,
                        //RedirectStandardInput = true
                    }
                })
                {
                    process.OutputDataReceived += Process_OutputDataReceived;
                    //process.ErrorDataReceived += Process_ErrorDataReceived;
                    process.Start();
                    process.WaitForExit();
                }
                
                NotifyNewConsoleMessageEvent($"Finished process with command: {pCommand}");
            });
        }
    }
}
