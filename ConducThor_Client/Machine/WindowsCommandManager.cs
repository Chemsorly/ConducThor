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
        public override void CreateProcess(WorkPackage.Command pCommands)
        {
            //return Task.Run(() =>
            //{
                //https://loune.net/2017/06/running-shell-bash-commands-in-net-core/
                NotifyNewConsoleMessageEvent($"[DEBUG] Create process for: {pCommands.FileName} {pCommands.Arguments} {pCommands.WorkDir}");
                using (var process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = pCommands.FileName,
                        WorkingDirectory = pCommands.WorkDir,
                        Arguments = pCommands.Arguments,
                        RedirectStandardOutput = true,
                        //RedirectStandardInput = true,
                        RedirectStandardError = true
                        //UseShellExecute = false
                    }
                })
                {
                    process.OutputDataReceived += Process_OutputDataReceived;
                    process.ErrorDataReceived += Process_ErrorDataReceived;
                    NotifyNewConsoleMessageEvent($"[DEBUG] Starting process for: {pCommands.FileName} {pCommands.Arguments} {pCommands.WorkDir}");
                    process.Start();
                    process.WaitForExit();
                    NotifyNewConsoleMessageEvent($"[DEBUG] Finished process for: {pCommands.FileName} {pCommands.Arguments} {pCommands.WorkDir}");
                }
            //});
        }
    }
}
