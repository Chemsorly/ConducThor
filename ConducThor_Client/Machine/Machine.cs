using System;
using System.Collections.Generic;
using System.Text;
using ConducThor_Shared;
using ConducThor_Shared.Enums;

namespace ConducThor_Client.Machine
{
    public class Machine
    {
        public static MachineData GetMachineData()
        {
            return new MachineData()
            {
                ContainerVersion = GetContainerVersion(),
                OperatingSystem = GetOperatingSystem(),
                ProcessingUnit = GetProcessingUnitType()
            };
        }

        static OSEnum GetOperatingSystem()
        {
            var osvar = Environment.GetEnvironmentVariable("CONDUCTHOR_OS");
            if (osvar == "ubuntu")
                return OSEnum.Ubuntu;
            else if (osvar == "windows")
                return OSEnum.Windows;
            else
                return OSEnum.undefined;
        }

        static ProcessingUnitEnum GetProcessingUnitType()
        {
            var puvar = Environment.GetEnvironmentVariable("CONDUCTHOR_TYPE");
            if (puvar == "cpu")
                return ProcessingUnitEnum.CPU;
            else if (puvar == "gpu")
                return ProcessingUnitEnum.GPU;
            else
                return ProcessingUnitEnum.undefined;
        }

        static String GetContainerVersion()
        {
            return Environment.GetEnvironmentVariable("CONDUCTHOR_VERSION") ?? "dev";
        }
    }
}
