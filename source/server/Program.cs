using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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
            MessageBus.MessageBusBroker broker = new MessageBus.MessageBusBroker("AshSample");
            broker.Start();

            Console.WriteLine("Broker active! Enter 'exit' to quit.");
            string command = string.Empty;
            while (!command.Equals("exit"))
            {
                command = Console.ReadLine();
                broker.SendMessage(command);
            }
        }
    }
}
