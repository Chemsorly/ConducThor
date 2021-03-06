﻿using System;
using System.Linq;
using System.Threading.Tasks;
using ConducThor_Client.Client;

namespace ConducThor_Client
{
    class Program
    {
        private static Client.SignalRManager _client;


        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("No args specified");
                return;
            }

            //fetch machine data from env variables
            var machinedata = Machine.Machine.GetMachineData();

            //container version
            Console.WriteLine($"Container version: {machinedata.ContainerVersion}");

            //start
            Console.WriteLine($"Target host: {args[0]}");
            Task.Factory.StartNew(() =>
            {
                _client = new SignalRManager(machinedata);
                _client.LogEvent += _client_LogEvent;
                _client.Initialize($"http://{CleanHoststring(args[0])}/signalr");
            });
            Console.ReadKey();
        }

        private static void _client_LogEvent(string message)
        {
            Console.WriteLine(message);
        }

        private static String CleanHoststring(String pInput)
        {
            //remove http://, https:// 
            return pInput.Replace("http://", String.Empty).Replace("https://", String.Empty);
        }
    }
}
