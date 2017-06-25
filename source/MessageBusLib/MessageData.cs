using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MessageBus
{
    /// <summary>
    /// Data we transmit between busses
    /// </summary>
    [Serializable]
    public class MessageData
    {
        private const string kNoArgs = "";
        public int Version = 1;
        public string Command;
        public string Args;

        public MessageData(string command, string args = kNoArgs)
        {
            Command = command;
            Args = args;
        }
    }

    /// <summary>
    /// Wrapper object for generating a Payload to be transmitted across the wire.
    /// </summary>
    public class MessagePayload
    {
        public static byte[] Encode(MessageData data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            formatter.Serialize(memoryStream, data);
            return memoryStream.ToArray();
        }

        public static MessageData Decode(byte[] data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MessageData message;
            MemoryStream ms = new MemoryStream(data);
            message = (MessageData)formatter.Deserialize(ms);
            return message;
        }
    }

    public class MessageEventArgs
    {
        public readonly MessageData Message;

        public MessageEventArgs(byte[] buffer)
        {
            Message = MessagePayload.Decode(buffer);
        }
    }

    public delegate void MessageEventHandler(object sender, MessageEventArgs args);
}
