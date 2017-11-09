using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConducThor_Server.Utility
{
    public abstract class ManagerClass
    {
        public delegate void NewLogMessage(String pLogMessage);

        public event NewLogMessage NewLogMessageEvent;

        protected void NotifyNewLogMessageEvent(String pMessage)
        {
            NewLogMessageEvent?.Invoke(pMessage);
        }
    }
}
