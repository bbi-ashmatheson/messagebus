using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageBus
{
    public class MessageEventArgs
    {
        public MessageData Message { get; set; }

        public MessageEventArgs(byte[] buffer)
        {
            Message = new MessageData(buffer);
        }
    }

    public delegate void MessageEventHandler(object sender, MessageEventArgs args);

    public abstract class NamedPipeStreamBase : IDisposable
    {
        private readonly Object mEventLock = new Object();
        private MessageEventHandler InternalMessageReceived;

        protected static readonly int kMaxBufferSize = 4096;

        public string PipeName { get; private set; }

        public NamedPipeStreamBase(string pipeName)
        {
            PipeName = pipeName;
        }

        ~NamedPipeStreamBase()
        {
            Dispose(false);
        }

        public virtual void Disconnect()
        {
            lock (mEventLock)
            {
                InternalMessageReceived = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }


        protected virtual void OnMessageReceived(MessageEventArgs args)
        {
            lock (mEventLock)
            {
                if (InternalMessageReceived != null)
                {
                    InternalMessageReceived(this, args);
                }
            }
        }

        public abstract void SendMessage(MessageData message);

        public event MessageEventHandler MessageReceived
        {
            add
            {
                lock (mEventLock)
                {
                    InternalMessageReceived = Delegate.Combine(InternalMessageReceived, value) as MessageEventHandler;
                }
            }

            remove
            {
                lock (mEventLock)
                {
                    InternalMessageReceived = Delegate.Remove(InternalMessageReceived, value) as MessageEventHandler;
                }
            }
        }
    }
}
