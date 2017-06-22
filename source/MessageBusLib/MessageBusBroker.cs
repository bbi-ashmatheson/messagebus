using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Net;
using System.Text;
using System.Threading;

namespace MessageBus
{
    public class MessageBusBroker
    {
        private string PipeName { get; set; }
        private List<NamedPipeStreamConnection> mConnections;

        public bool IsActive
        {
            get
            {
                return true;
            }
        }

        public MessageBusBroker(string name)
        {
            PipeName = name;
            mConnections = new List<NamedPipeStreamConnection>();
        }

        public void Start()
        {
            NamedPipeServerStream stream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, -1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            stream.BeginWaitForConnection(new AsyncCallback(ClientConnected), stream);
        }

        private void ClientConnected(IAsyncResult result)
        {
            NamedPipeServerStream asyncState = result.AsyncState as NamedPipeServerStream;
            asyncState.EndWaitForConnection(result);
            if (asyncState.IsConnected)
            {
                NamedPipeStreamConnection item = new NamedPipeStreamConnection(asyncState, PipeName);
                item.MessageReceived += new MessageEventHandler(Connection_MessageReceived);
                lock (mConnections)
                {
                    mConnections.Add(item);
                }
            }
            NamedPipeServerStream state = new NamedPipeServerStream(PipeName, PipeDirection.InOut, -1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            state.BeginWaitForConnection(new AsyncCallback(this.ClientConnected), state);
        }

        public void SendMessage(string command)
        {
            foreach (var connection in mConnections)
            {
                connection.SendMessage(new MessageData(command));
            }
        }
        private void Connection_MessageReceived(object sender, MessageEventArgs args)
        {
            Console.WriteLine(string.Format("Server received: {0}", args.Message.message.Command));
        }

        public void Stop()
        {
        }
    }
}
