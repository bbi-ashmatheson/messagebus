using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.Serialization.Formatters.Binary;

namespace MMF_Server
{
    public enum Mode
    {
        Unused = 0,
        Client = 1,
        Server = 2
    }

    [Serializable]
    public class SampleData
    {
        public Mode Destination { get; set; }
        public string Message { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            SampleData data = new SampleData()
            {
                Destination = Mode.Client,
                Message = "From Server"
            };

            byte[] writeBuffer = new byte[1024];
            byte[] readBuffer = new byte[1024];
            SharedMemory.CircularBuffer sendRing = new SharedMemory.CircularBuffer("Test", 50, writeBuffer.Length);
            SharedMemory.CircularBuffer readRing = new SharedMemory.CircularBuffer("TestReader", 50, writeBuffer.Length);
            long counter = 0;
            do
            {
                {
                    string message = string.Format("Test Message {0}", counter++);
                    BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream memoryStream = new MemoryStream();
                    formatter.Serialize(memoryStream, message);
                    writeBuffer = memoryStream.ToArray();
                    sendRing.Write(writeBuffer);
                }

                if (readRing.Read(readBuffer) > 0)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream memoryStream = new MemoryStream(readBuffer);
                    string readData = formatter.Deserialize(memoryStream) as string;
                    Console.WriteLine(string.Format("Found message: {0}", readData));
                }
            } while (true);
        }
    }
}
