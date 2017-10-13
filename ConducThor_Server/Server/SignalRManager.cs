using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;

namespace ConducThor_Server.Server
{
    class SignalRManager
    {
        private CommHub _hub;
        private IDisposable _signalrapp;

        public void Initialize()
        {
            _hub = new CommHub();
            _hub.Initialize();
            _signalrapp = WebApp.Start<SignalRStartup>("http://*:8080/");
        }

        #region owin code

        #endregion
    }
}
