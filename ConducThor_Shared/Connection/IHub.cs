﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ConducThor_Shared.Connection
{
    public interface IHub
    {
        /// <summary>
        /// initially sends information about machine data
        /// </summary>
        /// <param name="pMachineData">Machine data structure</param>
        void Connect(MachineData pMachineData);

        /// <summary>
        /// fetches a work unit from the server to process; TODO: define work unit
        /// </summary>
        void FetchWork();

        /// <summary>
        /// sends result to server after work unit is completed; TODO: define result unit
        /// </summary>
        void SendResults();

        /// <summary>
        /// updates the computing machines status
        /// </summary>
        /// <param name="pStatus">client status structure</param>
        void UpdateStatus(ClientStatus pStatus);
    }
}