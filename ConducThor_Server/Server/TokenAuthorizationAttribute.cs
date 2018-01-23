using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace ConducThor_Server.Server
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TokenAuthorizationAttribute : AuthorizeAttribute
    {
        public override bool AuthorizeHubConnection(Microsoft.AspNet.SignalR.Hubs.HubDescriptor hubDescriptor, IRequest request)
        {
            string token = request.Headers["authtoken"];
            if (token == $"ThisIsACustomTokenToPreventSpamBecauseFuckIt")
            {
                return true;
            }
            return false;
        }
    }
}
