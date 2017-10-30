using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using ConducThor_Shared;

namespace ConducThor_Client.Machine
{
    class LinuxCommandManager : ICommandManager
    {
        public override Task CreateProcess(WorkPackage.Command pCommands)
        {
            return Task.Factory.StartNew(() =>
            {
                //https://loune.net/2017/06/running-shell-bash-commands-in-net-core/
                using (var process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = pCommands.FileName,
                        WorkingDirectory = pCommands.WorkDir,
                        Arguments = pCommands.Arguments,
                        RedirectStandardOutput = true,
                        //RedirectStandardInput = true,
                        //RedirectStandardError = true,
                        UseShellExecute = false
                    }
                })
                {
                    process.OutputDataReceived += Process_OutputDataReceived;
                    //process.ErrorDataReceived += Process_ErrorDataReceived;
                    process.Start();
                    process.WaitForExit();
                }
            });
        }
    }
}
