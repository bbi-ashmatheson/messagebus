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
            Console.WriteLine("Beginning Client Startup");
            MessageBus.MessageBusNode clientNode = new MessageBus.MessageBusNode("AshSample");
            clientNode.AddChannel(new TestChannel());
            clientNode.Start();
            Console.WriteLine("Client active! Enter 'exit' to quit.");
            string command = string.Empty;
            while (!command.Equals("exit"))
            {
                command = Console.ReadLine();
                clientNode.SendMessage(new MessageData(command));
            }
        }
    }

    public class TestChannel : IMessageBusChannel
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
