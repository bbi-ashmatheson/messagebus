using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


namespace Assets.Scripts
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

    public class MMF_Client : MonoBehaviour
    {
        private SharedMemory.CircularBuffer mReadBuffer;
        private SharedMemory.CircularBuffer mWriteBuffer;
        private long counter = 0;
        void Start()
        {
            mReadBuffer = new SharedMemory.CircularBuffer("Test");
            mWriteBuffer = new SharedMemory.CircularBuffer("TestReader");
        }

        void Update()
        {
            byte[] readBuffer = new byte[1024];
            if (mReadBuffer.Read(readBuffer, 0, 0) > 0)
            {
                string result;
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream memoryStream = new MemoryStream(readBuffer);
                result = formatter.Deserialize(memoryStream) as string;
                Debug.Log(string.Format("Found message! {0}", result));
            }

            if (Input.GetKeyUp(KeyCode.T))
            {
                byte[] writeBuffer = new byte[1024];
                string message = string.Format("Test Message {0}", counter++);
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream memoryStream = new MemoryStream();
                formatter.Serialize(memoryStream, message);
                writeBuffer = memoryStream.ToArray();
                mWriteBuffer.Write(writeBuffer);

            }
        }
    }
}
