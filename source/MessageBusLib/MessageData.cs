using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MessageBus
{
    [Serializable]
    public struct MessagePayload
    {
        public int Length;
        public string Command;
    }

    public class MessageData
    {
        public MessagePayload message;

        public byte[] Buffer
        {
            get
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream memoryStream = new MemoryStream();
                formatter.Serialize(memoryStream, message);
                return memoryStream.ToArray();
            }
        }

        public MessageData(string command)
        {
            message = new MessagePayload
            {
                Length = command.Length,
                Command = command
            };
        }

        public MessageData(byte[] buffer)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            message = (MessagePayload)formatter.Deserialize(ms);
        }
    }
}
