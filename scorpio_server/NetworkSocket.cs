using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace scorpio_server
{
    class NetworkSocket
    {
        static void ProcessRequests(Socket socket)
        {
            byte[] buffer = new byte[1024];
            string message_queue = "";

            while (true)
            {
                int num_bytes_recived = socket.Receive(buffer);

                if (num_bytes_recived > 0)
                {
                    string msg = Encoding.ASCII.GetString(buffer, 0, num_bytes_recived);

                    string[] messages = msg.Split(new char[] { Constants.C_END_OF_MESSAGE }, StringSplitOptions.None);
                    messages[0] = message_queue + messages[0];
                    message_queue = messages.Last();

                    foreach (string str in messages.Take(messages.Length - 1))
                    {
                        Console.WriteLine("Message from client {0}:{1} - {2}", (socket.RemoteEndPoint as IPEndPoint).Address, (socket.RemoteEndPoint as IPEndPoint).Port, str);
                    }

                    string return_msg = msg.ToUpper();
                    byte[] return_msg_en = Encoding.ASCII.GetBytes(return_msg);
                    socket.Send(return_msg_en);
                }
            }
        }

        static void Ping(Socket socket)
        {
            byte[] ping_message = Encoding.ASCII.GetBytes("<PING>");
            socket.Send(ping_message);
        }

        public static void Run(Socket connection_socket)
        {
            CancellationToken cancelation_token = new CancellationToken();
            Task.Run(() => { ProcessRequests(connection_socket); }, cancelation_token);

            int last_second = DateTime.Now.Second;
            while (connection_socket.Connected)
            {
                if (DateTime.Now.Second != last_second && DateTime.Now.Second % 3 == 0)
                {
                    Ping(connection_socket);
                    last_second = DateTime.Now.Second;
                }
            }

            cancelation_token.ThrowIfCancellationRequested();
            Console.WriteLine("Client {0}:{1} disconnected!", (connection_socket.RemoteEndPoint as IPEndPoint).Address, (connection_socket.RemoteEndPoint as IPEndPoint).Port);
        }

        public static void Send(Socket socket, string message)
        {
            string msg = message + Constants.C_END_OF_MESSAGE;
            byte[] message_encoded = Encoding.ASCII.GetBytes(msg);

            socket.Send(message_encoded);
        }
    }
}
