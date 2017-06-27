using System;
using System.IO.Pipes;
using System.Diagnostics;
using System.Text;

namespace MessageBus
{
    public class Asynchronus_NamedPipe_Server
    {
        public readonly string pipe_address;
        private NamedPipeServerStream namedPipeServerStream;
        private string Server_Message;



        public delegate void ASYNC_pipe_status_callback(string message);


        private byte[] read_buffer = new byte[1024];
        private byte[] write_buffer = new byte[1024];


        public Asynchronus_NamedPipe_Server(string pipe_address)
        {
            try
            {

                this.pipe_address = pipe_address;
                namedPipeServerStream = new NamedPipeServerStream(this.pipe_address, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                Console.WriteLine("Connecting to Client...");
                namedPipeServerStream.WaitForConnection();
                Console.WriteLine("Connected to Client");
                Read_from_Client_Async();

            }
            catch (Exception oEX)
            {
                Console.WriteLine(oEX.Message);
            }
        }





        public void Write_to_Client_Async(string message)
        {
            if (namedPipeServerStream != null)
            {
                if (namedPipeServerStream.CanWrite && namedPipeServerStream.IsConnected)
                {
                    namedPipeServerStream.WaitForPipeDrain();
                    ASCIIEncoding.ASCII.GetBytes(message).CopyTo(write_buffer, 0);

                    namedPipeServerStream.BeginWrite(write_buffer, 0, write_buffer.Length, new AsyncCallback(Async_Write_Completed), 2);

                }
                else
                {
                    close_pipe();
                }
            }
        }



        public void Read_from_Client_Async()
        {

            if (namedPipeServerStream != null)
            {
                if (namedPipeServerStream.CanRead && namedPipeServerStream.IsConnected)
                {

                    namedPipeServerStream.BeginRead(read_buffer, 0, read_buffer.Length, new AsyncCallback(Async_Read_Completed), 1);
                }
                else
                {
                    close_pipe();
                }

            }
        }



        private void Async_Read_Completed(IAsyncResult result)
        {
            namedPipeServerStream.EndRead(result);

            this.Server_Message = ASCIIEncoding.ASCII.GetString(read_buffer);
            this.Server_Message.Trim();
            Console.WriteLine("Received from Client => " + this.Server_Message + " <=REnd");

            Read_from_Client_Async();

        }


        private void Async_Write_Completed(IAsyncResult result)
        {
            namedPipeServerStream.EndWrite(result);
            Debug.WriteLine("Written To Client => " + ASCIIEncoding.ASCII.GetString(write_buffer));


        }

        public Boolean Is_connected_to_server()
        {
            return this.namedPipeServerStream.IsConnected;
        }


        public void close_pipe()
        {
            if (namedPipeServerStream.IsConnected)
            {
                namedPipeServerStream.Disconnect();
            }
            namedPipeServerStream.Close();
            namedPipeServerStream.Dispose();

            Debug.WriteLine(" Pipe Closed");
        }
    }
}
