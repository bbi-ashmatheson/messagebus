using System;
using MessageBus;

namespace NamedPipesTest
{
    /// <summary>
    /// Server SERVER Server!!!
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Beginning Server Startup - press escape to quit");
            // MessageBus.MessageBusService service = new MessageBus.MessageBusService("AshSample");
            // service.Start();
            // 
            // Console.WriteLine("Broker active! Enter 'exit' to quit.");
            // string command = string.Empty;
            // while (!command.Equals("exit"))
            // {
            //     command = Console.ReadLine();
            //     service.SendMessage(command);
            // }
            // 
            // service.Stop();

            Asynchronus_NamedPipe_Server asyncServer = new Asynchronus_NamedPipe_Server("AshSample");
            while (true)
            {
                do
                {
                    asyncServer.Write_to_Client_Async("ack");
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            }

        }
    }
}
