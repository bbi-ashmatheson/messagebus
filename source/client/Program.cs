using MessageBus;
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
            Console.WriteLine("Beginning Client Startup - press escape to quit");
            // Old message bus code
            // MessageBus.MessageBusNode clientNode = new MessageBus.MessageBusNode("AshSample");
            // clientNode.AddChannel(new TestChannel());
            // clientNode.Start();
            // Console.WriteLine("Client active! Enter 'exit' to quit.");
            // string command = string.Empty;
            // while (!command.Equals("exit"))
            // {
            //     command = Console.ReadLine();
            //     clientNode.SendMessage(new MessageData(command));
            // }

            Asynchronus_NamedPipe_Client asyncClient = new Asynchronus_NamedPipe_Client("AshSample");

            while (asyncClient.Is_connected_to_server())
            {
                if (Console.ReadKey().Key != ConsoleKey.Escape)
                {
                    asyncClient.Write_to_Server_Async("NEX Sample");
                }
            }
        }
    }

    public class TestChannel : IMessageBusReceiver
    {
        public string Name
        {
            get
            {
                return "TestChannel";
            }
        }

        public void DoCommand(MessageData message)
        {
            Console.WriteLine(string.Format("TestChannel Handled Command {0}:{1}", message.Command, message.Args));
        }
    }
}
