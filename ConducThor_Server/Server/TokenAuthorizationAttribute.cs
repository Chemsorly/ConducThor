using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace ConducThor_Server.Server
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class TokenAuthorizationAttribute : AuthorizeAttribute
    {
        private readonly string AuthToken = $"ThisIsACustomTokenToPreventSpamBecauseFuckIt";

        public override bool AuthorizeHubConnection(Microsoft.AspNet.SignalR.Hubs.HubDescriptor hubDescriptor, IRequest request)
        {
            string token = request.Headers["authtoken"];
            if (token == AuthToken)
            {
                return true;
            }
            return false;
        }

        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            string token = hubIncomingInvokerContext.Hub.Context.Headers["authtoken"];
            if (token == AuthToken)
            {
                return true;
            }
            return false;
        }
    }
}
