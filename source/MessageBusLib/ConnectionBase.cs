using System;

namespace MessageBus
{
    /// <summary>
    /// Base class for handling input from a bi-directional communication protocol.
    /// </summary>
    public abstract class ConnectionBase : IDisposable
    {
        private readonly object mLock;
        private MessageEventHandler mMessageReceivedHandler;

        bool IsConnected;

        protected const int kMaxBufferSize = 4096;

        protected readonly string PipeName;

        public ConnectionBase(string name)
        {
            mLock = new object();
            PipeName = name;
        }

        ~ConnectionBase()
        {
            Dispose(false);
        }

        public virtual void Disconnect()
        {
            lock (mLock)
            {
                mMessageReceivedHandler = null;
            }
        }

        public virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        public abstract void SendMessage(MessageData data);

        public event MessageEventHandler MessageReceived
        {
            add
            {
                lock (mLock)
                {
                    mMessageReceivedHandler = Delegate.Combine(mMessageReceivedHandler, value) as MessageEventHandler;
                }
            }

            remove
            {
                lock (mLock)
                {
                    mMessageReceivedHandler = Delegate.Remove(mMessageReceivedHandler, value) as MessageEventHandler;
                }
            }
        }


        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void OnMessageReceived(MessageEventArgs args)
        {
            lock (mLock)
            {
                mMessageReceivedHandler?.Invoke(this, args);
            }
        }
    }
}
