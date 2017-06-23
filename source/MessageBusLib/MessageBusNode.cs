using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Net;
using System.Text;
using System.Threading;

namespace MessageBus
{
    public class MessageBusNode : NamedPipeStreamBase
    {
        private ManualResetEvent mConnectionGate;
        private readonly object mInstanceLock;
        private Queue<MessageData> mPendingMessages;
        private readonly object mQueueLock;
        private PipeStream mStream;

        public MessageBusNode(string pipeName) : base(pipeName)
        {
            mInstanceLock = new object();
            mQueueLock = new object();
            mConnectionGate = new ManualResetEvent(false);
        }

        public void Start()
        {
            StartTryConnect();
        }

        private void StartTryConnect()
        {
            mConnectionGate.Reset();
            Thread thread = new Thread(new ThreadStart(this.TryConnect));
            thread.Name = "NamedPipeStreamClientConnection";
            thread.IsBackground = true;
            thread.Start();
        }

        private void TryConnect()
        {
            mConnectionGate.Reset();
            lock (mInstanceLock)
            {
                mStream = new NamedPipeClientStream(".", base.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

                while (!mStream.IsConnected)
                {
                    try
                    {
                        ((NamedPipeClientStream)mStream).Connect(0x3e8);
                    }
                    catch
                    {
                    }
                }
                mStream.ReadMode = PipeTransmissionMode.Message;
                byte[] buffer = new byte[kMaxBufferSize];
                mStream.BeginRead(buffer, 0, kMaxBufferSize, new AsyncCallback(this.EndRead), buffer);
                mConnectionGate.Set();
                MessageReceived += Connection_MessageReceived;
                this.SendQueuedMessages();
            }
        }

        private void EndRead(IAsyncResult result)
        {
            // this can throw an exception when it first starts up, so...
            try
            {
                int length = mStream.EndRead(result);
                byte[] asyncState = (byte[])result.AsyncState;
                if (length > 0)
                {
                    byte[] destinationArray = new byte[length];
                    Array.Copy(asyncState, 0, destinationArray, 0, length);
                    this.OnMessageReceived(new MessageEventArgs(destinationArray));
                }
                lock (mInstanceLock)
                {
                    mStream.BeginRead(asyncState, 0, kMaxBufferSize, new AsyncCallback(this.EndRead), asyncState);
                }
            }
            catch
            {
            }

        }

        private void EndSendMessage(IAsyncResult result)
        {
            lock (mInstanceLock)
            {
                mStream.EndWrite(result);
                mStream.Flush();
            }
        }

        private void EnqueMessage(MessageData message)
        {
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
        public override void Disconnect()
        {
            lock (mInstanceLock)
            {
                base.Disconnect();
                mStream.Close();
            }
        }

        public bool IsConnected
        {
            get
            {
                lock (mInstanceLock)
                {
                    return mStream.IsConnected;
                }
            }
        }

        public override void SendMessage(MessageData message)
        {
            if (mConnectionGate.WaitOne(100))
            {
                lock (mInstanceLock)
                {
                    if (mStream.IsConnected)
                    {
                        byte[] messagebuffer = message.Buffer;
                        mStream.BeginWrite(messagebuffer, 0, messagebuffer.Length, new AsyncCallback(this.EndSendMessage), null);
                        mStream.Flush();
                    }
                    else
                    {
                        EnqueMessage(message);
                        StartTryConnect();
                    }
                }
            }
            else
            {
                EnqueMessage(message);
            }
        }

        private void Connection_MessageReceived(object sender, MessageEventArgs args)
        {
            Console.WriteLine(string.Format("Client received: {0}", args.Message.message.Command));
        }

    }
}
