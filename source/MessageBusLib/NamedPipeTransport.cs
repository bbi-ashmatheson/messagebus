using System;
using System.IO.Pipes;

namespace MessageBus
{
    /// <summary>
    /// Transport layer for named pipes
    /// </summary>
    public class NamedPipeTransport : ConnectionBase
    {
        private readonly object mNamedPipeLock;
        private PipeStream mStream;

        public NamedPipeTransport(PipeStream stream, string name) : base(name)
        {
            mNamedPipeLock = new object();
            mStream = stream;
            byte[] buffer = new byte[kMaxBufferSize];
            mStream.BeginRead(buffer, 0, kMaxBufferSize, new AsyncCallback(EndRead), buffer);
        }

        public override void SendMessage(MessageData message)
        {
            byte[] buffer = MessageSerializer.Encode(message);
            lock (mNamedPipeLock)
            {
                if (mStream.IsConnected)
                {
                    mStream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(EndSendMessage), null);
                }
            }
        }

        public bool IsConnected
        {
            get
            {
                lock (mNamedPipeLock)
                {
                    return mStream.IsConnected;
                }
            }
        }

        public override void Disconnect()
        {
            mStream.Close();
            base.Disconnect();
        }

        private void EndRead(IAsyncResult result)
        {
            int length = mStream.EndRead(result);
            byte[] asyncState = (byte[])result.AsyncState;
            if (length > 0)
            {
                byte[] destinationArray = new byte[length];
                Array.Copy(asyncState, 0, destinationArray, 0, length);
                OnMessageReceived(new MessageEventArgs(destinationArray));
            }
            lock (mNamedPipeLock)
            {
                mStream.BeginRead(asyncState, 0, kMaxBufferSize, new AsyncCallback(EndRead), asyncState);
            }
        }

        private void EndSendMessage(IAsyncResult result)
        {
            lock (mNamedPipeLock)
            {
                mStream.EndWrite(result);
                mStream.Flush();
            }
        }
    }
}
