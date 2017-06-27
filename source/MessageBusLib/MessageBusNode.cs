using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;

namespace MessageBus
{
    /// <summary>
    /// There should only be one MessageBusNode per process for best use.
    /// </summary>
    public class MessageBusNode
    {
        private ManualResetEvent mConnectionGate;
        private Queue<MessageData> mPendingMessages;
        private List<IMessageBusReceiver> mChannels;
        private NamedPipeTransport mTransport;
        private Thread mQueuedMessageWorker;

        private readonly object mInstanceLock;
        private readonly object mQueueLock;
        private readonly string PipeName;

        public MessageBusNode(string name)
        {
            mInstanceLock = new object();
            mQueueLock = new object();
            mConnectionGate = new ManualResetEvent(false);
            mChannels = new List<IMessageBusReceiver>();
            PipeName = name;
        }

        public void Start()
        {
            ConnectToTransport();
        }

        public void Stop()
        {
            lock (mQueueLock)
            {
                if (mPendingMessages.Count > 0)
                {
                    Console.WriteLine("MessageBusNode shutting down. {0} messages outstanding", mPendingMessages.Count);
                    mPendingMessages.Clear();
                }
            }
        }

        public void SendMessage(MessageData message)
        {
            lock (mInstanceLock)
            {
                if (mTransport.IsConnected)
                {
                    mTransport.SendMessage(message);
                }
                else
                {
                    EnqueMessage(message);
                    ConnectToTransport();
                }
            }
        }

        public void AddChannel(IMessageBusReceiver channel)
        {
            mChannels.Add(channel);
        }

        private void ConnectToTransport()
        {
            mConnectionGate.Reset();
            Thread thread = new Thread(new ThreadStart(TryConnect));
            thread.Name = "MessageBusNode_Connection";
            thread.IsBackground = true;
            thread.Start();

            mQueuedMessageWorker = new Thread(QueueWorker);
        }

        private void TryConnect()
        {
            mConnectionGate.Reset();
            lock (mInstanceLock)
            {
                NamedPipeClientStream connection = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

                while (!connection.IsConnected)
                {
                    try
                    {
                        connection.Connect(0x3e8);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("Exception caught - MessageBusNode.TryConnect: {0}", ex.Message));
                    }
                }

                // Currently not pooling buffers - may want to do this at some point.
                connection.ReadMode = PipeTransmissionMode.Message;

                mTransport = new NamedPipeTransport(connection, PipeName);
                mTransport.MessageReceived += OnMessageReceived;
            }
        }

        private void QueueWorker()
        {
            while (mTransport.IsConnected)
            {
                SendQueuedMessages();
                Thread.Sleep(100);
            }
        }

        private void EnqueMessage(MessageData message)
        {
            // If we ever get out of .net 3.5 land, look into thread-safe collections 
            // (https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/index).
            lock (mQueueLock)
            {
                if (mPendingMessages == null)
                {
                    mPendingMessages = new Queue<MessageData>();
                }
                mPendingMessages.Enqueue(message);
            }
        }

        private void SendQueuedMessages()
        {
            lock (mQueueLock)
            {
                if (mPendingMessages != null)
                {
                    while (mPendingMessages.Count > 0)
                    {
                        SendMessage(mPendingMessages.Dequeue());
                    }
                    mPendingMessages = null;
                }
            }
        }

        private void OnMessageReceived(object sender, MessageEventArgs args)
        {
            foreach (var item in mChannels)
            {
                item.DoCommand(args.Message);
            }
            Console.WriteLine(string.Format("Client received: {0}", args.Message.Command));
        }
    }
}
