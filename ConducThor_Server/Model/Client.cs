﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConducThor_Shared.Enums;

namespace ConducThor_Server.Model
{
    public class Client
    {
        public String ID { get; set; }
        public String MachineName { get; set; }
        public String ContainerVersion { get; set; }
        public OSEnum OperatingSystem { get; set; }
        public ProcessingUnitEnum ProcessingUnit { get; set; }
    }
}