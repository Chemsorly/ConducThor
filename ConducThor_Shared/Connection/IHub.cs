using System;
using System.Collections.Generic;
using System.Text;

namespace ConducThor_Shared.Connection
{
    public interface IHub
    {
        void Connect();

        void FetchWork();

        void SendResults();

        void Pong(ClientStatus pStatus);
    }
}
