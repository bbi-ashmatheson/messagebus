using System;
using System.Text;
using System.IO.Pipes;
using System.Diagnostics;

namespace MessageBus
{
    public class Asynchronus_NamedPipe_Client
    {
        public readonly string pipe_address;
        public bool filter_message = true;

        private NamedPipeClientStream clientStream;
        private string Server_Message = null;


        public event ASYNC_pipe_status_callback ASYNC_external_Write_Completed;
        public event ASYNC_pipe_status_callback ASYNC_external_Read_Completed;
        public delegate void ASYNC_pipe_status_callback(string message);


        private byte[] read_buffer = new byte[1024];
        private byte[] write_buffer = new byte[1024];

        private IAsyncResult read_result;
        private IAsyncResult write_result;

        private int read_id = 1;

        public Asynchronus_NamedPipe_Client(string pipe_address)
        {
            try
            {
                this.pipe_address = pipe_address;
                //  if(clientStream.IsConnected){UnityEngine.Debug.Log("Server Already Running");}else{}
                clientStream = new NamedPipeClientStream(".", this.pipe_address, PipeDirection.InOut, PipeOptions.Asynchronous);


                clientStream.Connect(1);
                if (clientStream.IsConnected)
                {
                    Console.WriteLine("Connected to Server");
                    Read_from_Server_Async();
                }
                else
                {
                    Console.WriteLine("Could NOT connect to Server");
                }
            }
            catch (Exception oEX)
            {
                Console.WriteLine("Application Pipe Error: " + oEX.Message);
            }
        }



        public void Write_to_Server_Async(string message)
        {
            if (clientStream != null)
            {
                if (clientStream.CanWrite && clientStream.IsConnected)
                {
                    clientStream.WaitForPipeDrain();
                    ASCIIEncoding.ASCII.GetBytes(message).CopyTo(write_buffer, 0);
                    clientStream.BeginWrite(write_buffer, 0, write_buffer.Length, new AsyncCallback(Async_Write_Completed), 1);
                }
                else
                {
                    close_pipe();
                }
            }

        }



        public void Read_from_Server_Async()
        {
            if (clientStream.CanRead && clientStream.IsConnected)
            {
                clientStream.BeginRead(read_buffer, 0, read_buffer.Length, new AsyncCallback(Async_Read_Completed), 2);
            }
            else
            {
                close_pipe();
            }

        }



        private void Async_Write_Completed(IAsyncResult result)
        {
            clientStream.EndWrite(result);
            Console.WriteLine("Written To Server => " + ASCIIEncoding.ASCII.GetString(write_buffer));
        }



        private void Async_Read_Completed(IAsyncResult result)
        {
            clientStream.EndRead(result);
            Server_Message = ASCIIEncoding.ASCII.GetString(read_buffer);
            this.Server_Message.Trim();
            Console.WriteLine("Received from Server => " + Server_Message);
            Debug.WriteLine("Received from Server => " + Server_Message);
            if (clientStream.CanRead && clientStream.IsConnected)
            {
                Read_from_Server_Async();
            }
            else
            {
                close_pipe();
            }

        }

        public Boolean Is_connected_to_server()
        {
            return clientStream.IsConnected;
        }



        public void close_pipe()
        {
            if (clientStream != null)
            {
                if (clientStream.IsConnected)
                {
                    clientStream.Close();
                    clientStream.Dispose();

                    Console.WriteLine(" Pipe Closed");
                }
            }

        }
    }
}
