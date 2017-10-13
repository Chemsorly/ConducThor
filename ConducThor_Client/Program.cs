using System;
using System.Threading.Tasks;
using ConducThor_Client.Client;

namespace ConducThor_Client
{
    class Program
    {
        private static Client.SignalRManager _client;


        static void Main(string[] args)
        {
            Console.WriteLine("Hello World2!");

            Task.Factory.StartNew(() =>
            {
                _client = new SignalRManager();
                _client.LogEvent += _client_LogEvent;
                _client.Initialize("http://localhost:8080/signalr");
            });
            Console.ReadKey();
        }

        private static void _client_LogEvent(string message)
        {
            Console.WriteLine(message);
        }
    }
}
