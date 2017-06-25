using System;
using System.Collections.Generic;
using System.IO.Pipes;

namespace MessageBus
{
    /// <summary>
    /// This is a 'service' that needs to run on at least one 'process' to create and manage the underlying transport mecanism.
    /// ie: if the transport was HTTP, this would instantiate a web service Transport
    /// Sice we currently only work with Named Pipes, it contains a list of NamedPipeConnections
    /// </summary>
    public class MessageBusService
    {
        private string PipeName { get; set; }
        private List<NamedPipeTransport> mConnections;

        public bool IsActive
        {
            get
            {
                return true;
            }
        }

        public MessageBusService(string name)
        {
            PipeName = name;
            mConnections = new List<NamedPipeTransport>();
        }

        public void Start()
        {
            NamedPipeServerStream stream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, -1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            stream.BeginWaitForConnection(new AsyncCallback(ClientConnected), stream);
        }

        public void SendMessage(string command)
        {
            foreach (var connection in mConnections)
            {
                connection.SendMessage(new MessageData(command));
            }
        }

        public void Stop()
        {
            foreach (var connection in mConnections)
            {
                connection.Disconnect();
            }

            mConnections.Clear();
        }

        private void ClientConnected(IAsyncResult result)
        {
            NamedPipeServerStream asyncState = result.AsyncState as NamedPipeServerStream;
            asyncState.EndWaitForConnection(result);
            if (asyncState.IsConnected)
            {
                NamedPipeTransport item = new NamedPipeTransport(asyncState, PipeName);
                item.MessageReceived += new MessageEventHandler(OnMessageReceived);
                lock (mConnections)
                {
                    mConnections.Add(item);
                }
            }
            NamedPipeServerStream state = new NamedPipeServerStream(PipeName, PipeDirection.InOut, -1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            state.BeginWaitForConnection(new AsyncCallback(this.ClientConnected), state);
        }

        private void OnMessageReceived(object sender, MessageEventArgs args)
        {
            Console.WriteLine(string.Format("Server received: {0}", args.Message.Command));
        }
    }
}
