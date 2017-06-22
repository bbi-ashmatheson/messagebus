using System;

namespace client
{
    class Program
    {
        /// <summary>
        /// Client CLIENT Client!!!
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Beginning Client Startup");
            MessageBus.MessageBusNode clientNode = new MessageBus.MessageBusNode("AshSample");
            clientNode.Start();
            Console.WriteLine("Client active! Enter 'exit' to quit.");
            string command = string.Empty;
            while (!command.Equals("exit"))
            {
                command = Console.ReadLine();
                clientNode.SendMessage(new MessageBus.MessageData(command));
            }
        }
    }
}
