using System;

namespace NamedPipesTest
{
    /// <summary>
    /// Server SERVER Server!!!
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Beginning Server Startup");
            MessageBus.MessageBusService service = new MessageBus.MessageBusService("AshSample");
            service.Start();

            Console.WriteLine("Broker active! Enter 'exit' to quit.");
            string command = string.Empty;
            while (!command.Equals("exit"))
            {
                command = Console.ReadLine();
                service.SendMessage(command);
            }

            service.Stop();
        }
    }
}
