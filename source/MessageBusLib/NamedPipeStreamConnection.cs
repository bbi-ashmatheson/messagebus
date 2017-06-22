using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace MessageBus
{
    public class NamedPipeStreamConnection : NamedPipeStreamBase
    {
        private readonly object mInstanceLock;
        private PipeStream mStream;

        public NamedPipeStreamConnection(PipeStream stream, string name) : base(name)
        {
            mInstanceLock = new object();
            mStream = stream;
            byte[] buffer = new byte[kMaxBufferSize];
            mStream.BeginRead(buffer, 0, kMaxBufferSize, new AsyncCallback(EndRead), buffer);
        }

        public override void SendMessage(MessageData message)
        {
            byte[] buffer = message.Buffer;
            lock (mInstanceLock)
            {
                if (mStream.IsConnected)
                {
                    mStream.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(EndSendMessage), null);
                }
            }
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
            lock (mInstanceLock)
            {
                mStream.BeginRead(asyncState, 0, kMaxBufferSize, new AsyncCallback(EndRead), asyncState);
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
    }
}
