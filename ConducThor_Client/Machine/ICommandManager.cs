using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ConducThor_Client.Client;
using ConducThor_Shared;

namespace ConducThor_Client.Machine
{
    public abstract class ICommandManager
    {
        internal event SignalRManager.NewConsoleMessage NewConsoleMessageEvent;

        public abstract void CreateProcess(WorkPackage.Command pCommands);

        public void NotifyNewConsoleMessageEvent(String pMessage)
        {
            NewConsoleMessageEvent?.BeginInvoke(pMessage, null, null);
        }

        internal void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            NotifyNewConsoleMessageEvent(e.Data);
        }

        internal void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            NotifyNewConsoleMessageEvent(e.Data);
        }
    }
}
