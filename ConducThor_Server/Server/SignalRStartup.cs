﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Owin;

namespace ConducThor_Server.Server
{
    public class SignalRStartup
    {
        public static IAppBuilder App = null;

        public void Configuration(IAppBuilder app)
        {
            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = null;
            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                var hubConfiguration = new HubConfiguration
                {
                    EnableDetailedErrors = true,
                    EnableJSONP = true
                };
                GlobalHost.DependencyResolver = hubConfiguration.Resolver;
                GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = null;
                GlobalHost.Configuration.DefaultMessageBufferSize = 1000;
                map.RunSignalR(hubConfiguration);
            });
        }
    }
}
